using Xunit;
using FluentAssertions;
using NSubstitute;
using Obss.Orders.Application.Abstractions;
using Obss.Orders.Application.Commands.SubmitProductOrder;
using Obss.Orders.Domain.Entities;
using Obss.Orders.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Tests.Application;

public class SubmitProductOrderCommandHandlerTests
{
    private readonly IProductOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly SubmitProductOrderCommandHandler _handler;

    public SubmitProductOrderCommandHandlerTests()
    {
        _orderRepository = Substitute.For<IProductOrderRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new SubmitProductOrderCommandHandler(_orderRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ShouldSubmitOrder_WhenOrderExistsAndHasItems()
    {
        var order = CreateOrderWithItems();
        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        var result = await _handler.Handle(new SubmitProductOrderCommand(order.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Submitted);
        await _orderRepository.Received(1).UpdateAsync(order, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenOrderNotFound_ShouldReturnNotFound()
    {
        var orderId = Guid.NewGuid();
        _orderRepository.GetByIdAsync(orderId, Arg.Any<CancellationToken>())
            .Returns((ProductOrder?)null);

        var result = await _handler.Handle(new SubmitProductOrderCommand(orderId), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task Handle_WhenOrderHasNoItems_ShouldReturnValidationError()
    {
        var order = ProductOrder.Create(
            "tenant-1", Guid.NewGuid(), "John Doe",
            OrderType.New, "user-1");
        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        var result = await _handler.Handle(new SubmitProductOrderCommand(order.Id), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.Validation");
    }

    [Fact]
    public async Task Handle_WhenOrderAlreadySubmitted_ShouldReturnValidationError()
    {
        var order = CreateOrderWithItems();
        order.Submit();
        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        var result = await _handler.Handle(new SubmitProductOrderCommand(order.Id), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.Validation");
    }

    private static ProductOrder CreateOrderWithItems()
    {
        var order = ProductOrder.Create(
            "tenant-1", Guid.NewGuid(), "John Doe",
            OrderType.New, "user-1");
        order.AddItem(
            Guid.NewGuid(), Guid.NewGuid(),
            "Internet", "Basic Plan",
            1, 49.99m, 29.99m, 5m, 8m,
            BillingPeriod.Monthly);
        return order;
    }
}
