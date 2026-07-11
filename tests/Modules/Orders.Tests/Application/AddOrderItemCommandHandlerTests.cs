using Xunit;
using FluentAssertions;
using NSubstitute;
using Obss.Orders.Application.Abstractions;
using Obss.Orders.Application.Commands.AddProductOrderItem;
using Obss.Orders.Domain.Entities;
using Obss.Orders.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Tests.Application;

public class AddProductOrderItemCommandHandlerTests
{
    private readonly IProductOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AddProductOrderItemCommandHandler _handler;

    public AddProductOrderItemCommandHandlerTests()
    {
        _orderRepository = Substitute.For<IProductOrderRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new AddProductOrderItemCommandHandler(_orderRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ShouldAddItem_WhenOrderExistsAndIsDraft()
    {
        var order = CreateDraftOrder();
        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        var command = new AddProductOrderItemCommand(
            order.Id, Guid.NewGuid(), Guid.NewGuid(),
            "New Product", "New Offer",
            2, 79.99m, 39.99m, 10m, 15m,
            "Monthly", DateTime.UtcNow, null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        order.Items.Should().HaveCount(2);
        await _orderRepository.Received(1).UpdateAsync(order, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenOrderNotFound_ShouldReturnNotFound()
    {
        var orderId = Guid.NewGuid();
        _orderRepository.GetByIdAsync(orderId, Arg.Any<CancellationToken>())
            .Returns((ProductOrder?)null);

        var command = new AddProductOrderItemCommand(
            orderId, Guid.NewGuid(), Guid.NewGuid(),
            "Product", "Offer", 1, 10m, 0, 0, 0,
            "Monthly", null, null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task Handle_WhenOrderNotDraft_ShouldReturnValidationError()
    {
        var order = CreateSubmittedOrder();
        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        var command = new AddProductOrderItemCommand(
            order.Id, Guid.NewGuid(), Guid.NewGuid(),
            "Product", "Offer", 1, 10m, 0, 0, 0,
            "Monthly", null, null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.Validation");
    }

    [Fact]
    public async Task Handle_WhenBillingPeriodInvalid_ShouldReturnValidationError()
    {
        var order = CreateDraftOrder();
        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        var command = new AddProductOrderItemCommand(
            order.Id, Guid.NewGuid(), Guid.NewGuid(),
            "Product", "Offer", 1, 10m, 0, 0, 0,
            "InvalidPeriod", null, null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.Validation");
        result.Error.Description.Should().Contain("Invalid billing period");
    }

    private static ProductOrder CreateDraftOrder()
    {
        var order = ProductOrder.Create(
            "tenant-1", Guid.NewGuid(), "John Doe",
            OrderType.New, "user-1");
        order.AddItem(
            Guid.NewGuid(), Guid.NewGuid(),
            "Internet", "Basic Plan",
            1, 49.99m, 0, 0, 0,
            BillingPeriod.Monthly);
        return order;
    }

    private static ProductOrder CreateSubmittedOrder()
    {
        var order = CreateDraftOrder();
        order.Submit();
        return order;
    }
}
