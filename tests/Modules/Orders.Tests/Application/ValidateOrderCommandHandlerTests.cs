using Xunit;
using FluentAssertions;
using NSubstitute;
using Obss.Orders.Application.Abstractions;
using Obss.Orders.Application.Commands.ValidateOrder;
using Obss.Orders.Application.Services;
using Obss.Orders.Domain.Entities;
using Obss.Orders.Domain.ValueObjects;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Tests.Application;

public class ValidateOrderCommandHandlerTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly OrderValidationService _validationService;
    private readonly ValidateOrderCommandHandler _handler;

    public ValidateOrderCommandHandlerTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();
        _validationService = Substitute.For<OrderValidationService>(
            Substitute.For<Obss.CRM.Application.Abstractions.ICustomerRepository>(),
            Substitute.For<Obss.ProductCatalog.Application.Abstractions.IProductRepository>(),
            Substitute.For<Obss.ProductCatalog.Application.Abstractions.IOfferRepository>());
        _handler = new ValidateOrderCommandHandler(_orderRepository, _validationService);
    }

    [Fact]
    public async Task Handle_ShouldReturnValidationResult_WhenOrderExists()
    {
        var order = Order.Create(
            "tenant-1", Guid.NewGuid(), "John Doe",
            OrderType.New, "user-1");
        _orderRepository.GetByIdWithItemsAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);
        var expectedResult = new OrderValidationResult(true, [], []);
        _validationService.ValidateAsync(order, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var result = await _handler.Handle(
            new ValidateOrderCommand(order.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenOrderNotFound_ShouldReturnNotFound()
    {
        var orderId = Guid.NewGuid();
        _orderRepository.GetByIdWithItemsAsync(orderId, Arg.Any<CancellationToken>())
            .Returns((Order?)null);

        var result = await _handler.Handle(
            new ValidateOrderCommand(orderId), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.NotFound");
    }
}
