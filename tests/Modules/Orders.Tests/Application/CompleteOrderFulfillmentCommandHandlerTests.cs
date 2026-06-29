using Xunit;
using FluentAssertions;
using NSubstitute;
using Obss.Orders.Application.Abstractions;
using Obss.Orders.Application.Commands.CompleteOrderFulfillment;
using Obss.Orders.Domain.Entities;
using Obss.Orders.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Tests.Application;

public class CompleteOrderFulfillmentCommandHandlerTests
{
    private readonly IOrderFulfillmentRepository _fulfillmentRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CompleteOrderFulfillmentCommandHandler _handler;

    public CompleteOrderFulfillmentCommandHandlerTests()
    {
        _fulfillmentRepository = Substitute.For<IOrderFulfillmentRepository>();
        _orderRepository = Substitute.For<IOrderRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new CompleteOrderFulfillmentCommandHandler(
            _fulfillmentRepository, _orderRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ShouldCompleteFulfillmentAndOrder_WhenSuccessful()
    {
        var order = CreateFulfillingOrder();
        var fulfillment = CreateInProgressFulfillment(order.Id);
        _fulfillmentRepository.GetByOrderIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(fulfillment);
        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        var result = await _handler.Handle(
            new CompleteOrderFulfillmentCommand(order.Id, true, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        fulfillment.Status.Should().Be(FulfillmentStatus.Completed);
        fulfillment.CompletedAt.Should().NotBeNull();
        order.Status.Should().Be(OrderStatus.Completed);

        await _fulfillmentRepository.Received(1).UpdateAsync(fulfillment, Arg.Any<CancellationToken>());
        await _orderRepository.Received(1).UpdateAsync(order, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldFailFulfillment_WhenNotSuccessful()
    {
        var order = CreateFulfillingOrder();
        var fulfillment = CreateInProgressFulfillment(order.Id);
        _fulfillmentRepository.GetByOrderIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(fulfillment);

        var result = await _handler.Handle(
            new CompleteOrderFulfillmentCommand(order.Id, false, "Provisioning error"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        fulfillment.Status.Should().Be(FulfillmentStatus.Failed);
        fulfillment.ErrorMessage.Should().Be("Provisioning error");
    }

    [Fact]
    public async Task Handle_WhenFulfillmentNotFound_ShouldReturnNotFound()
    {
        var orderId = Guid.NewGuid();
        _fulfillmentRepository.GetByOrderIdAsync(orderId, Arg.Any<CancellationToken>())
            .Returns((OrderFulfillment?)null);

        var result = await _handler.Handle(
            new CompleteOrderFulfillmentCommand(orderId, true, null), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    private static Order CreateFulfillingOrder()
    {
        var order = Order.Create(
            "tenant-1", Guid.NewGuid(), "John Doe",
            OrderType.New, "user-1");
        order.AddItem(
            Guid.NewGuid(), Guid.NewGuid(),
            "Internet", "Basic Plan",
            1, 49.99m, 0, 0, 0,
            BillingPeriod.Monthly);
        order.Submit();
        order.Approve("approver-1");
        order.StartFulfillment();
        return order;
    }

    private static OrderFulfillment CreateInProgressFulfillment(Guid orderId)
    {
        var fulfillment = OrderFulfillment.Create(orderId);
        fulfillment.StartFulfillment(Guid.NewGuid());
        return fulfillment;
    }
}
