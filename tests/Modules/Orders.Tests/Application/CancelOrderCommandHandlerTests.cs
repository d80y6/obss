using Xunit;
using FluentAssertions;
using NSubstitute;
using Obss.Orders.Application.Abstractions;
using Obss.Orders.Application.Commands.CancelOrder;
using Obss.Orders.Domain.Entities;
using Obss.Orders.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Tests.Application;

public class CancelOrderCommandHandlerTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CancelOrderCommandHandler _handler;

    public CancelOrderCommandHandlerTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new CancelOrderCommandHandler(_orderRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ShouldCancelOrder_WhenOrderExists()
    {
        var order = CreateDraftOrder();
        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        var result = await _handler.Handle(
            new CancelOrderCommand(order.Id, "Customer request"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Cancelled);
        order.CancellationReason.Should().Be("Customer request");
        await _orderRepository.Received(1).UpdateAsync(order, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenOrderNotFound_ShouldReturnNotFound()
    {
        var orderId = Guid.NewGuid();
        _orderRepository.GetByIdAsync(orderId, Arg.Any<CancellationToken>())
            .Returns((Order?)null);

        var result = await _handler.Handle(
            new CancelOrderCommand(orderId, "Reason"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task Handle_WhenOrderCompleted_ShouldReturnValidationError()
    {
        var order = CreateCompletedOrder();
        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        var result = await _handler.Handle(
            new CancelOrderCommand(order.Id, "Too late"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.Validation");
    }

    private static Order CreateDraftOrder()
    {
        var order = Order.Create(
            "tenant-1", Guid.NewGuid(), "John Doe",
            OrderType.New, "user-1");
        order.AddItem(
            Guid.NewGuid(), Guid.NewGuid(),
            "Internet", "Basic Plan",
            1, 49.99m, 0, 0, 0,
            BillingPeriod.Monthly);
        return order;
    }

    private static Order CreateCompletedOrder()
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
        order.MarkCompleted();
        return order;
    }
}
