using Xunit;
using FluentAssertions;
using Obss.Orders.Domain.Entities;
using Obss.Orders.Domain.Events;
using Obss.Orders.Domain.ValueObjects;
using Obss.Orders.Domain.Exceptions;

namespace Obss.Orders.Tests.Domain;

public class ProductOrderItemStateMachineTests
{
    private static void CreateOrderWithItem(out ProductOrderItem item)
    {
        var order = ProductOrder.Create("tenant1", Guid.NewGuid(), "Test", OrderType.New, "user1", null, null, null, "USD");
        var productId = Guid.NewGuid();
        var offerId = Guid.NewGuid();
        order.AddItem(productId, offerId, "Product", "Offer", 1, 100, 0, 0, 0, BillingPeriod.Monthly);
        item = order.Items.ElementAt(0);
    }

    [Fact]
    public void Acknowledge_DefaultState_StaysAcknowledged()
    {
        CreateOrderWithItem(out var item);
        item.State.Should().Be(ProductOrderItemState.Acknowledged);
    }

    [Fact]
    public void StartProgress_FromAcknowledged_TransitionsToInProgress()
    {
        CreateOrderWithItem(out var item);
        item.StartProgress();
        item.State.Should().Be(ProductOrderItemState.InProgress);
    }

    [Fact]
    public void StartProgress_FromCompleted_Throws()
    {
        CreateOrderWithItem(out var item);
        item.StartProgress();
        item.Assess();
        item.Complete();
        var act = () => item.StartProgress();
        act.Should().Throw<InvalidProductOrderItemStateException>();
    }

    [Fact]
    public void FullLifecycle_AcknowledgeToComplete_Succeeds()
    {
        CreateOrderWithItem(out var item);
        item.StartProgress();
        item.Assess();
        item.Complete();
        item.State.Should().Be(ProductOrderItemState.Completed);
    }

    [Fact]
    public void Hold_And_Resume_Succeeds()
    {
        CreateOrderWithItem(out var item);
        item.StartProgress();
        item.Hold();
        item.State.Should().Be(ProductOrderItemState.Held);
        item.Resume();
        item.State.Should().Be(ProductOrderItemState.InProgress);
    }

    [Fact]
    public void Pending_And_Resume_Succeeds()
    {
        CreateOrderWithItem(out var item);
        item.StartProgress();
        item.Pending("awaiting stock");
        item.State.Should().Be(ProductOrderItemState.Pending);
        item.StartProgress();
        item.State.Should().Be(ProductOrderItemState.InProgress);
    }

    [Fact]
    public void Assess_Then_Reject_Succeeds()
    {
        CreateOrderWithItem(out var item);
        item.StartProgress();
        item.Assess();
        item.Reject("not eligible");
        item.State.Should().Be(ProductOrderItemState.Rejected);
    }

    [Fact]
    public void Fail_FromInProgress_TransitionsToFailed()
    {
        CreateOrderWithItem(out var item);
        item.StartProgress();
        item.Fail("processing error");
        item.State.Should().Be(ProductOrderItemState.Failed);
    }

    [Fact]
    public void Cancel_FromInProgress_TransitionsToCancelled()
    {
        CreateOrderWithItem(out var item);
        item.StartProgress();
        item.Cancel();
        item.State.Should().Be(ProductOrderItemState.Cancelled);
    }

    [Fact]
    public void Cancel_FromCompleted_Throws()
    {
        CreateOrderWithItem(out var item);
        item.StartProgress();
        item.Assess();
        item.Complete();
        var act = () => item.Cancel();
        act.Should().Throw<InvalidProductOrderItemStateException>();
    }

    [Fact]
    public void StateChangedEvent_RaisedOnTransition()
    {
        CreateOrderWithItem(out var item);
        item.State.Should().Be(ProductOrderItemState.Acknowledged);
        item.StartProgress();
        item.State.Should().Be(ProductOrderItemState.InProgress);
        var events = item.DomainEvents.ToList();
        events.Should().ContainSingle(e => e is ProductOrderItemStateChangedDomainEvent);
    }

    [Fact]
    public void SameStateTransition_DoesNotRaiseEvent()
    {
        CreateOrderWithItem(out var item);
        item.StartProgress();
        item.Hold();
        item.Resume();
        var act = () => item.StartProgress();
        act.Should().Throw<InvalidProductOrderItemStateException>();
    }
}
