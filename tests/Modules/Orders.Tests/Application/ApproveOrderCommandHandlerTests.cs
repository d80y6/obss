using Xunit;
using FluentAssertions;
using NSubstitute;
using Obss.Orders.Application.Abstractions;
using Obss.Orders.Application.Commands.ApproveOrder;
using Obss.Orders.Domain.Entities;
using Obss.Orders.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Tests.Application;

public class ApproveOrderCommandHandlerTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApproveOrderCommandHandler _handler;

    public ApproveOrderCommandHandlerTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();
        _currentUser = Substitute.For<ICurrentUser>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new ApproveOrderCommandHandler(_orderRepository, _currentUser, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ShouldApproveOrder_WhenOrderExistsAndIsSubmitted()
    {
        var order = CreateSubmittedOrder();
        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);
        _currentUser.UserId.Returns("approver-1");

        var result = await _handler.Handle(new ApproveOrderCommand(order.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Approved);
        order.ApprovedById.Should().Be("approver-1");
        await _orderRepository.Received(1).UpdateAsync(order, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenOrderNotFound_ShouldReturnNotFound()
    {
        var orderId = Guid.NewGuid();
        _orderRepository.GetByIdAsync(orderId, Arg.Any<CancellationToken>())
            .Returns((Order?)null);

        var result = await _handler.Handle(new ApproveOrderCommand(orderId), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task Handle_WhenOrderNotSubmitted_ShouldReturnValidationError()
    {
        var order = Order.Create(
            "tenant-1", Guid.NewGuid(), "John Doe",
            OrderType.New, "user-1");
        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        var result = await _handler.Handle(new ApproveOrderCommand(order.Id), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.Validation");
    }

    private static Order CreateSubmittedOrder()
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
        return order;
    }
}
