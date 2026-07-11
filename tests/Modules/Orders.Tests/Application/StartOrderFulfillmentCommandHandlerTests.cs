using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Obss.Orders.Application.Abstractions;
using Obss.Orders.Application.Commands.StartOrderFulfillment;
using Obss.Orders.Domain.Entities;
using Obss.Orders.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Tests.Application;

public class StartOrderFulfillmentCommandHandlerTests
{
    private readonly IProductOrderRepository _orderRepository;
    private readonly IOrderFulfillmentRepository _fulfillmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<StartOrderFulfillmentCommandHandler> _logger;
    private readonly StartOrderFulfillmentCommandHandler _handler;

    public StartOrderFulfillmentCommandHandlerTests()
    {
        _orderRepository = Substitute.For<IProductOrderRepository>();
        _fulfillmentRepository = Substitute.For<IOrderFulfillmentRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _logger = Substitute.For<ILogger<StartOrderFulfillmentCommandHandler>>();
        _handler = new StartOrderFulfillmentCommandHandler(
            _orderRepository, _fulfillmentRepository, _unitOfWork, _logger);
    }

    [Fact]
    public async Task Handle_ShouldStartFulfillment_WhenOrderIsApproved()
    {
        var order = CreateApprovedOrder();
        _orderRepository.GetByIdWithItemsAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);
        _fulfillmentRepository.GetByOrderIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns((OrderFulfillment?)null);

        var result = await _handler.Handle(
            new StartOrderFulfillmentCommand(order.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Status.Should().Be("InProgress");
        await _fulfillmentRepository.Received(1).AddAsync(Arg.Any<OrderFulfillment>(), Arg.Any<CancellationToken>());
        await _orderRepository.Received(1).UpdateAsync(order, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenOrderNotFound_ShouldReturnNotFound()
    {
        var orderId = Guid.NewGuid();
        _orderRepository.GetByIdWithItemsAsync(orderId, Arg.Any<CancellationToken>())
            .Returns((ProductOrder?)null);

        var result = await _handler.Handle(
            new StartOrderFulfillmentCommand(orderId), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task Handle_WhenFulfillmentAlreadyExists_ShouldReturnValidationError()
    {
        var order = CreateApprovedOrder();
        _orderRepository.GetByIdWithItemsAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);
        _fulfillmentRepository.GetByOrderIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(OrderFulfillment.Create(order.Id));

        var result = await _handler.Handle(
            new StartOrderFulfillmentCommand(order.Id), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.Validation");
        result.Error.Description.Should().Contain("already exists");
    }

    [Fact]
    public async Task Handle_WhenOrderNotApproved_ShouldReturnValidationError()
    {
        var order = CreateDraftOrder();
        _orderRepository.GetByIdWithItemsAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);
        _fulfillmentRepository.GetByOrderIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns((OrderFulfillment?)null);

        var result = await _handler.Handle(
            new StartOrderFulfillmentCommand(order.Id), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.Validation");
    }

    private static ProductOrder CreateApprovedOrder()
    {
        var order = ProductOrder.Create(
            "tenant-1", Guid.NewGuid(), "John Doe",
            OrderType.New, "user-1");
        order.AddItem(
            Guid.NewGuid(), Guid.NewGuid(),
            "Internet", "Basic Plan",
            1, 49.99m, 0, 0, 0,
            BillingPeriod.Monthly);
        order.Submit();
        order.Approve("approver-1");
        return order;
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
}
