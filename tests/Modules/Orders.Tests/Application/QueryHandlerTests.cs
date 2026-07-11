using Xunit;
using FluentAssertions;
using NSubstitute;
using Obss.Orders.Application.Abstractions;
using Obss.Orders.Application.Contracts;
using Obss.Orders.Application.DTOs;
using Obss.Orders.Application.Queries.GetProductOrderById;
using Obss.Orders.Application.Queries.GetProductOrders;
using Obss.Orders.Application.Queries.GetProductOrdersByCustomer;
using Obss.Orders.Application.Queries.GetOrderFulfillmentStatus;
using Obss.Orders.Domain.Entities;
using Obss.Orders.Domain.ValueObjects;

namespace Obss.Orders.Tests.Application;

public class QueryHandlerTests
{
    [Fact]
    public async Task GetOrderById_ShouldReturnOrder_WhenOrderExists()
    {
        var repository = Substitute.For<IProductOrderRepository>();
        var order = ProductOrder.Create(
            "tenant-1", Guid.NewGuid(), "John Doe",
            OrderType.New, "user-1");
        repository.GetByIdWithItemsAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);
        var handler = new GetProductOrderByIdQueryHandler(repository);

        var result = await handler.Handle(
            new GetProductOrderByIdQuery(order.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(order.Id);
        result.Value.CustomerName.Should().Be("John Doe");
    }

    [Fact]
    public async Task GetOrderById_WhenOrderNotFound_ShouldReturnNotFound()
    {
        var repository = Substitute.For<IProductOrderRepository>();
        var orderId = Guid.NewGuid();
        repository.GetByIdWithItemsAsync(orderId, Arg.Any<CancellationToken>())
            .Returns((ProductOrder?)null);
        var handler = new GetProductOrderByIdQueryHandler(repository);

        var result = await handler.Handle(
            new GetProductOrderByIdQuery(orderId), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task GetOrders_ShouldReturnFilteredList()
    {
        var repository = Substitute.For<IProductOrderRepository>();
        var orders = new List<ProductOrder>
        {
            ProductOrder.Create("tenant-1", Guid.NewGuid(), "Alice", OrderType.New, "user-1"),
            ProductOrder.Create("tenant-1", Guid.NewGuid(), "Bob", OrderType.Renewal, "user-1")
        };
        repository.GetFilteredAsync(
                Arg.Any<Guid?>(), Arg.Any<OrderStatus?>(),
                Arg.Any<DateTime?>(), Arg.Any<DateTime?>(),
                Arg.Any<string?>(), Arg.Any<string?>(),
                Arg.Any<int>(), Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns(orders);
        repository.GetCountAsync(
                Arg.Any<Guid?>(), Arg.Any<OrderStatus?>(),
                Arg.Any<DateTime?>(), Arg.Any<DateTime?>(),
                Arg.Any<CancellationToken>())
            .Returns(orders.Count);
        var handler = new GetProductOrdersQueryHandler(repository);

        var result = await handler.Handle(
            new GetProductOrdersQuery(null, null, null, null, null, null, 1, 20),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetOrdersByCustomer_ShouldReturnCustomerOrders()
    {
        var repository = Substitute.For<IProductOrderRepository>();
        var customerId = Guid.NewGuid();
        var orders = new List<ProductOrder>
        {
            ProductOrder.Create("tenant-1", customerId, "Alice", OrderType.New, "user-1")
        };
        repository.GetByCustomerAsync(customerId, Arg.Any<int>(), Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns(orders);
        repository.GetCountAsync(customerId, Arg.Any<OrderStatus?>(),
                Arg.Any<DateTime?>(), Arg.Any<DateTime?>(),
                Arg.Any<CancellationToken>())
            .Returns(orders.Count);
        var handler = new GetProductOrdersByCustomerQueryHandler(repository);

        var result = await handler.Handle(
            new GetProductOrdersByCustomerQuery(customerId, 1, 20),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
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
