using Xunit;
using FluentAssertions;
using NSubstitute;
using Obss.Orders.Application.Abstractions;
using Obss.Orders.Application.DTOs;
using Obss.Orders.Application.Queries.GetOrderById;
using Obss.Orders.Application.Queries.GetOrders;
using Obss.Orders.Application.Queries.GetOrdersByCustomer;
using Obss.Orders.Application.Queries.GetOrderFulfillmentStatus;
using Obss.Orders.Domain.Entities;
using Obss.Orders.Domain.ValueObjects;

namespace Obss.Orders.Tests.Application;

public class QueryHandlerTests
{
    [Fact]
    public async Task GetOrderById_ShouldReturnOrder_WhenOrderExists()
    {
        var repository = Substitute.For<IOrderRepository>();
        var order = Order.Create(
            "tenant-1", Guid.NewGuid(), "John Doe",
            OrderType.New, "user-1");
        repository.GetByIdWithItemsAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);
        var handler = new GetOrderByIdQueryHandler(repository);

        var result = await handler.Handle(
            new GetOrderByIdQuery(order.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(order.Id);
        result.Value.CustomerName.Should().Be("John Doe");
    }

    [Fact]
    public async Task GetOrderById_WhenOrderNotFound_ShouldReturnNotFound()
    {
        var repository = Substitute.For<IOrderRepository>();
        var orderId = Guid.NewGuid();
        repository.GetByIdWithItemsAsync(orderId, Arg.Any<CancellationToken>())
            .Returns((Order?)null);
        var handler = new GetOrderByIdQueryHandler(repository);

        var result = await handler.Handle(
            new GetOrderByIdQuery(orderId), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task GetOrders_ShouldReturnFilteredList()
    {
        var repository = Substitute.For<IOrderRepository>();
        var orders = new List<Order>
        {
            Order.Create("tenant-1", Guid.NewGuid(), "Alice", OrderType.New, "user-1"),
            Order.Create("tenant-1", Guid.NewGuid(), "Bob", OrderType.Renewal, "user-1")
        };
        repository.GetFilteredAsync(
                Arg.Any<Guid?>(), Arg.Any<OrderStatus?>(),
                Arg.Any<DateTime?>(), Arg.Any<DateTime?>(),
                Arg.Any<string?>(), Arg.Any<string?>(),
                Arg.Any<int>(), Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns(orders);
        var handler = new GetOrdersQueryHandler(repository);

        var result = await handler.Handle(
            new GetOrdersQuery(null, null, null, null, null, null, 1, 20),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetOrdersByCustomer_ShouldReturnCustomerOrders()
    {
        var repository = Substitute.For<IOrderRepository>();
        var customerId = Guid.NewGuid();
        var orders = new List<Order>
        {
            Order.Create("tenant-1", customerId, "Alice", OrderType.New, "user-1")
        };
        repository.GetByCustomerAsync(customerId, Arg.Any<int>(), Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns(orders);
        var handler = new GetOrdersByCustomerQueryHandler(repository);

        var result = await handler.Handle(
            new GetOrdersByCustomerQuery(customerId, 1, 20),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetOrderFulfillmentStatus_ShouldReturnFulfillment_WhenExists()
    {
        var repository = Substitute.For<IOrderFulfillmentRepository>();
        var orderId = Guid.NewGuid();
        var fulfillment = OrderFulfillment.Create(orderId);
        repository.GetByOrderIdAsync(orderId, Arg.Any<CancellationToken>())
            .Returns(fulfillment);
        var handler = new GetOrderFulfillmentStatusQueryHandler(repository);

        var result = await handler.Handle(
            new GetOrderFulfillmentStatusQuery(orderId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.OrderId.Should().Be(orderId);
    }

    [Fact]
    public async Task GetOrderFulfillmentStatus_WhenNotFound_ShouldReturnNotFound()
    {
        var repository = Substitute.For<IOrderFulfillmentRepository>();
        var orderId = Guid.NewGuid();
        repository.GetByOrderIdAsync(orderId, Arg.Any<CancellationToken>())
            .Returns((OrderFulfillment?)null);
        var handler = new GetOrderFulfillmentStatusQueryHandler(repository);

        var result = await handler.Handle(
            new GetOrderFulfillmentStatusQuery(orderId), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.NotFound");
    }
}
