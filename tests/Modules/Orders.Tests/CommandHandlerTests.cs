using Xunit;
using FluentAssertions;
using NSubstitute;
using Obss.Orders.Application.Abstractions;
using Obss.Orders.Application.Commands.AddOrderItem;
using Obss.Orders.Application.Commands.ApproveOrder;
using Obss.Orders.Application.Commands.CancelOrder;
using Obss.Orders.Application.Commands.CompleteOrderFulfillment;
using Obss.Orders.Application.Commands.StartOrderFulfillment;
using Obss.Orders.Application.Commands.SubmitOrder;
using Obss.Orders.Domain.Entities;
using Obss.Orders.Domain.ValueObjects;
using Obss.Orders.Infrastructure.Persistence;
using Obss.Orders.Infrastructure.Persistence.Repositories;
using Obss.SharedKernel.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace Obss.Orders.Tests;

public class CommandHandlerTests : OrdersIntegrationTests
{
    [Fact]
    public async Task SubmitOrderCommand_ShouldSubmitOrderInDatabase()
    {
        using var context = CreateDbContext();
        var orderRepository = new OrderRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var order = Order.Create(
            "test-tenant", Guid.NewGuid(), "John Doe",
            OrderType.New, "test-user");
        order.AddItem(
            Guid.NewGuid(), Guid.NewGuid(),
            "Internet Plan", "Basic 100Mbps",
            1, 49.99m, 29.99m, 5m, 8m,
            BillingPeriod.Monthly);
        await orderRepository.AddAsync(order);
        await context.SaveChangesAsync();

        var handler = new SubmitOrderCommandHandler(orderRepository, unitOfWork);

        var result = await handler.Handle(
            new SubmitOrderCommand(order.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var saved = await orderRepository.GetByIdWithItemsAsync(order.Id);
        saved.Should().NotBeNull();
        saved!.Status.Should().Be(OrderStatus.Submitted);
    }

    [Fact]
    public async Task SubmitOrderCommand_WithNoItems_ShouldReturnFailure()
    {
        using var context = CreateDbContext();
        var orderRepository = new OrderRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var order = Order.Create(
            "test-tenant", Guid.NewGuid(), "John Doe",
            OrderType.New, "test-user");
        await orderRepository.AddAsync(order);
        await context.SaveChangesAsync();

        var handler = new SubmitOrderCommandHandler(orderRepository, unitOfWork);

        var result = await handler.Handle(
            new SubmitOrderCommand(order.Id), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.Validation");
    }

    [Fact]
    public async Task SubmitAndApproveOrderCommand_ShouldApproveOrder()
    {
        using var context = CreateDbContext();
        var orderRepository = new OrderRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var order = Order.Create(
            "test-tenant", Guid.NewGuid(), "Jane Doe",
            OrderType.New, "test-user");
        order.AddItem(
            Guid.NewGuid(), Guid.NewGuid(),
            "Fiber Optic", "Fiber 1Gbps",
            1, 79.99m, 0, 0, 0,
            BillingPeriod.Monthly);
        await orderRepository.AddAsync(order);
        await context.SaveChangesAsync();

        var submitHandler = new SubmitOrderCommandHandler(orderRepository, unitOfWork);
        await submitHandler.Handle(new SubmitOrderCommand(order.Id), CancellationToken.None);

        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns("approver-1");
        var approveHandler = new ApproveOrderCommandHandler(
            orderRepository, currentUser, unitOfWork);

        var result = await approveHandler.Handle(
            new ApproveOrderCommand(order.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var saved = await orderRepository.GetByIdWithItemsAsync(order.Id);
        saved!.Status.Should().Be(OrderStatus.Approved);
        saved.ApprovedById.Should().Be("approver-1");
    }

    [Fact]
    public async Task SubmitApproveAndCancelOrderCommand_ShouldCancelOrder()
    {
        using var context = CreateDbContext();
        var orderRepository = new OrderRepository(context);
        var unitOfWork = CreateUnitOfWork(context);
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns("approver-1");

        var order = CreateSavedOrder(context);

        var submitHandler = new SubmitOrderCommandHandler(orderRepository, unitOfWork);
        await submitHandler.Handle(new SubmitOrderCommand(order.Id), CancellationToken.None);

        var approveHandler = new ApproveOrderCommandHandler(
            orderRepository, currentUser, unitOfWork);
        await approveHandler.Handle(new ApproveOrderCommand(order.Id), CancellationToken.None);

        var cancelHandler = new CancelOrderCommandHandler(orderRepository, unitOfWork);
        var result = await cancelHandler.Handle(
            new CancelOrderCommand(order.Id, "Customer changed mind"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var saved = await orderRepository.GetByIdAsync(order.Id);
        saved!.Status.Should().Be(OrderStatus.Cancelled);
        saved.CancellationReason.Should().Be("Customer changed mind");
    }

    [Fact]
    public async Task FullOrderLifecycle_ShouldTransitionThroughAllStatuses()
    {
        using var context = CreateDbContext();
        var orderRepository = new OrderRepository(context);
        var fulfillmentRepository = new OrderFulfillmentRepository(context);
        var unitOfWork = CreateUnitOfWork(context);
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns("approver-1");
        var logger = Substitute.For<ILogger<StartOrderFulfillmentCommandHandler>>();

        var order = CreateSavedOrder(context);

        var submitHandler = new SubmitOrderCommandHandler(orderRepository, unitOfWork);
        await submitHandler.Handle(new SubmitOrderCommand(order.Id), CancellationToken.None);

        var approveHandler = new ApproveOrderCommandHandler(
            orderRepository, currentUser, unitOfWork);
        await approveHandler.Handle(new ApproveOrderCommand(order.Id), CancellationToken.None);

        var fulfillmentHandler = new StartOrderFulfillmentCommandHandler(
            orderRepository, fulfillmentRepository, unitOfWork, logger);
        await fulfillmentHandler.Handle(
            new StartOrderFulfillmentCommand(order.Id), CancellationToken.None);

        var completeHandler = new CompleteOrderFulfillmentCommandHandler(
            fulfillmentRepository, orderRepository, unitOfWork);
        await completeHandler.Handle(
            new CompleteOrderFulfillmentCommand(order.Id, true, null), CancellationToken.None);

        var saved = await orderRepository.GetByIdWithItemsAsync(order.Id);
        saved!.Status.Should().Be(OrderStatus.Completed);

        var savedFulfillment = await fulfillmentRepository.GetByOrderIdAsync(order.Id);
        savedFulfillment!.Status.Should().Be(FulfillmentStatus.Completed);
    }

    [Fact]
    public async Task AddOrderItemCommand_ShouldAddItemToDraftOrder()
    {
        using var context = CreateDbContext();
        var orderRepository = new OrderRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var order = Order.Create(
            "test-tenant", Guid.NewGuid(), "John Doe",
            OrderType.New, "test-user");
        await orderRepository.AddAsync(order);
        await context.SaveChangesAsync();

        var handler = new AddOrderItemCommandHandler(orderRepository, unitOfWork);
        var command = new AddOrderItemCommand(
            order.Id, Guid.NewGuid(), Guid.NewGuid(),
            "New Product", "New Offer",
            3, 29.99m, 9.99m, 5m, 4m,
            "Monthly", DateTime.UtcNow, null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var saved = await orderRepository.GetByIdWithItemsAsync(order.Id);
        saved!.Items.Should().HaveCount(1);
        saved.Items.Single().ProductName.Should().Be("New Product");
        saved.Items.Single().Quantity.Should().Be(3);
    }

    private static Order CreateSavedOrder(OrderDbContext context)
    {
        var order = Order.Create(
            "test-tenant", Guid.NewGuid(), "Jane Doe",
            OrderType.New, "test-user");
        order.AddItem(
            Guid.NewGuid(), Guid.NewGuid(),
            "Fiber Optic", "Fiber 1Gbps",
            1, 79.99m, 0, 0, 0,
            BillingPeriod.Monthly);
        context.Orders.Add(order);
        context.SaveChanges();
        return order;
    }
}
