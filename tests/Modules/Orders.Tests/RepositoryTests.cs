using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Obss.Orders.Domain.Entities;
using Obss.Orders.Domain.ValueObjects;
using Obss.Orders.Infrastructure.Persistence;
using Obss.Orders.Infrastructure.Persistence.Repositories;

namespace Obss.Orders.Tests;

public class RepositoryTests : OrdersIntegrationTests
{
    [Fact]
    public async Task CanAddAndRetrieveOrder()
    {
        using var context = CreateDbContext();
        var repository = new ProductOrderRepository(context);

        var order = ProductOrder.Create(
            "test-tenant", Guid.NewGuid(), "John Doe",
            OrderType.New, "test-user");

        await repository.AddAsync(order);
        await context.SaveChangesAsync();

        var retrieved = await repository.GetByIdAsync(order.Id);

        retrieved.Should().NotBeNull();
        retrieved!.OrderNumber.Should().NotBeNullOrEmpty();
        retrieved.CustomerName.Should().Be("John Doe");
        retrieved.Status.Should().Be(OrderStatus.Draft);
        retrieved.OrderType.Should().Be(OrderType.New);
        retrieved.TenantId.Should().Be("test-tenant");
    }

    [Fact]
    public async Task CanAddAndRetrieveOrderWithItems()
    {
        using var context = CreateDbContext();
        var repository = new ProductOrderRepository(context);

        var order = ProductOrder.Create(
            "test-tenant", Guid.NewGuid(), "John Doe",
            OrderType.New, "test-user");
        order.AddItem(
            Guid.NewGuid(), Guid.NewGuid(),
            "Internet Plan", "Basic 100Mbps",
            2, 49.99m, 29.99m, 5m, 8m,
            BillingPeriod.Monthly);
        await repository.AddAsync(order);
        await context.SaveChangesAsync();

        var retrieved = await repository.GetByIdWithItemsAsync(order.Id);

        retrieved.Should().NotBeNull();
        retrieved!.Items.Should().HaveCount(1);
        retrieved.Items.Single().ProductName.Should().Be("Internet Plan");
        retrieved.Items.Single().Quantity.Should().Be(2);
    }

    [Fact]
    public async Task CanRetrieveOrderByCustomer()
    {
        var customerId = Guid.NewGuid();

        using (var context1 = CreateDbContext())
        {
            var repository1 = new ProductOrderRepository(context1);
            var order1 = ProductOrder.Create(
                "test-tenant", customerId, "Alice",
                OrderType.New, "user-1");
            await repository1.AddAsync(order1);
            await context1.SaveChangesAsync();
        }

        using (var context2 = CreateDbContext())
        {
            var repository2 = new ProductOrderRepository(context2);
            var order2 = ProductOrder.Create(
                "test-tenant", customerId, "Bob",
                OrderType.Renewal, "user-1");
            await repository2.AddAsync(order2);
            await context2.SaveChangesAsync();
        }

        using (var context3 = CreateDbContext())
        {
            var repository3 = new ProductOrderRepository(context3);
            var orders = await repository3.GetByCustomerAsync(customerId, 1, 10);

            orders.Should().HaveCount(2);
            orders.Should().Contain(o => o.CustomerName == "Alice");
            orders.Should().Contain(o => o.CustomerName == "Bob");
        }
    }

    [Fact]
    public async Task CanFilterOrdersByStatus()
    {
        using (var context1 = CreateDbContext())
        {
            var repo1 = new ProductOrderRepository(context1);
            var draft = ProductOrder.Create("test-tenant", Guid.NewGuid(), "Draft Order", OrderType.New, "user-1");
            draft.AddItem(Guid.NewGuid(), Guid.NewGuid(), "Item", "Offer", 1, 10m, 0, 0, 0, BillingPeriod.Monthly);
            await repo1.AddAsync(draft);
            await context1.SaveChangesAsync();

            draft.Submit();
            await context1.SaveChangesAsync();

            var pending = ProductOrder.Create("test-tenant", Guid.NewGuid(), "Pending Order", OrderType.Change, "user-1");
            pending.AddItem(Guid.NewGuid(), Guid.NewGuid(), "Item", "Offer", 1, 10m, 0, 0, 0, BillingPeriod.Monthly);
            await repo1.AddAsync(pending);
            await context1.SaveChangesAsync();
        }

        using (var context2 = CreateDbContext())
        {
            var repo2 = new ProductOrderRepository(context2);
            var submittedOrders = await repo2.GetFilteredAsync(
                null, OrderStatus.Submitted, null, null, null, null, 1, 10);

            submittedOrders.Should().HaveCount(1);
            submittedOrders.Single().CustomerName.Should().Be("Draft Order");
        }
    }

    [Fact]
    public async Task CanAddAndRetrieveFulfillment()
    {
        using var context = CreateDbContext();
        var orderRepository = new ProductOrderRepository(context);
        var fulfillmentRepository = new OrderFulfillmentRepository(context);

        var order = ProductOrder.Create("test-tenant", Guid.NewGuid(), "John", OrderType.New, "user-1");
        order.AddItem(Guid.NewGuid(), Guid.NewGuid(), "Item", "Offer", 1, 10m, 0, 0, 0, BillingPeriod.Monthly);
        await orderRepository.AddAsync(order);
        await context.SaveChangesAsync();

        var fulfillment = OrderFulfillment.Create(order.Id);
        await fulfillmentRepository.AddAsync(fulfillment);
        await context.SaveChangesAsync();

        var retrieved = await fulfillmentRepository.GetByOrderIdAsync(order.Id);

        retrieved.Should().NotBeNull();
        retrieved!.OrderId.Should().Be(order.Id);
        retrieved.Status.Should().Be(FulfillmentStatus.Pending);
    }

    [Fact]
    public async Task CanQueryFulfillmentsByStatus()
    {
        using var context = CreateDbContext();
        var orderRepository = new ProductOrderRepository(context);
        var repository = new OrderFulfillmentRepository(context);

        var order1 = ProductOrder.Create("test-tenant", Guid.NewGuid(), "John", OrderType.New, "user-1");
        order1.AddItem(Guid.NewGuid(), Guid.NewGuid(), "Item", "Offer", 1, 10m, 0, 0, 0, BillingPeriod.Monthly);
        var order2 = ProductOrder.Create("test-tenant", Guid.NewGuid(), "Jane", OrderType.New, "user-1");
        order2.AddItem(Guid.NewGuid(), Guid.NewGuid(), "Item", "Offer", 1, 10m, 0, 0, 0, BillingPeriod.Monthly);
        await orderRepository.AddAsync(order1);
        await orderRepository.AddAsync(order2);
        await context.SaveChangesAsync();

        var f1 = OrderFulfillment.Create(order1.Id);
        var f2 = OrderFulfillment.Create(order2.Id);
        await repository.AddAsync(f1);
        await repository.AddAsync(f2);
        await context.SaveChangesAsync();

        var pending = await repository.GetByStatusAsync(FulfillmentStatus.Pending);

        pending.Should().HaveCount(2);
    }

    [Fact]
    public async Task CanSearchOrdersByCustomerName()
    {
        using (var context1 = CreateDbContext())
        {
            var repo1 = new ProductOrderRepository(context1);
            var order1 = ProductOrder.Create("test-tenant", Guid.NewGuid(), "Alice Johnson", OrderType.New, "user-1");
            order1.AddItem(Guid.NewGuid(), Guid.NewGuid(), "Item", "Offer", 1, 10m, 0, 0, 0, BillingPeriod.Monthly);
            await repo1.AddAsync(order1);
            await context1.SaveChangesAsync();
        }

        using (var context2 = CreateDbContext())
        {
            var repo2 = new ProductOrderRepository(context2);
            var result = await repo2.GetFilteredAsync(
                null, null, null, null, null, "Alice", 1, 10);

            result.Should().ContainSingle(o => o.CustomerName == "Alice Johnson");
        }
    }

    [Fact]
    public async Task CanUpdateOrder()
    {
        using var context = CreateDbContext();
        var repository = new ProductOrderRepository(context);

        var order = ProductOrder.Create(
            "test-tenant", Guid.NewGuid(), "John Doe",
            OrderType.New, "test-user");
        order.AddItem(Guid.NewGuid(), Guid.NewGuid(), "Item", "Offer", 1, 10m, 0, 0, 0, BillingPeriod.Monthly);
        await repository.AddAsync(order);
        await context.SaveChangesAsync();

        order.Submit();
        await repository.UpdateAsync(order);
        await context.SaveChangesAsync();

        var retrieved = await repository.GetByIdAsync(order.Id);
        retrieved!.Status.Should().Be(OrderStatus.Submitted);
    }
}
