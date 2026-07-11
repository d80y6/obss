using Xunit;
using FluentAssertions;
using Obss.Orders.Domain.Entities;
using Obss.Orders.Domain.Events;
using Obss.Orders.Domain.ValueObjects;

namespace Obss.Orders.Tests.Domain;

public class ProductOrderTests
{
    private static ProductOrder CreateDraftOrder()
    {
        return ProductOrder.Create(
            "tenant-1",
            Guid.NewGuid(),
            "John Doe",
            OrderType.New,
            "user-1",
            "Test order",
            null,
            null,
            "USD");
    }

    private static ProductOrder CreateOrderWithItems()
    {
        var order = CreateDraftOrder();
        order.AddItem(
            Guid.NewGuid(), Guid.NewGuid(),
            "Internet Plan", "Basic 100Mbps",
            1, 49.99m, 49.99m, 5m, 10m,
            BillingPeriod.Monthly);
        return order;
    }

    [Fact]
    public void Create_ShouldReturnOrderWithDraftStatus()
    {
        var order = CreateDraftOrder();

        order.Status.Should().Be(OrderStatus.Draft);
        order.OrderNumber.Should().NotBeNullOrEmpty();
        order.OrderNumber.Should().StartWith("ORD-");
        order.Items.Should().BeEmpty();
        order.Payments.Should().BeEmpty();
    }

    [Fact]
    public void Create_ShouldSetPropertiesCorrectly()
    {
        var customerId = Guid.NewGuid();
        var order = ProductOrder.Create(
            "tenant-1", customerId, "Jane Doe",
            OrderType.Renewal, "user-2",
            "Renewal order",
            null, null, "EUR");

        order.TenantId.Should().Be("tenant-1");
        order.CustomerId.Should().Be(customerId);
        order.CustomerName.Should().Be("Jane Doe");
        order.OrderType.Should().Be(OrderType.Renewal);
        order.CreatedById.Should().Be("user-2");
        order.Notes.Should().Be("Renewal order");
        order.Currency.Should().Be("EUR");
        order.SubTotal.Should().Be(0);
        order.GrandTotal.Should().Be(0);
    }

    [Fact]
    public void AddItem_ShouldAddItemAndRecalculateTotals()
    {
        var order = CreateDraftOrder();
        var productId = Guid.NewGuid();
        var offerId = Guid.NewGuid();

        order.AddItem(
            productId, offerId,
            "Fiber Optic", "Fiber 1Gbps",
            2, 79.99m, 39.99m, 10m, 15m,
            BillingPeriod.Monthly);

        order.Items.Should().HaveCount(1);
        var item = order.Items.Single();
        item.ProductId.Should().Be(productId);
        item.OfferId.Should().Be(offerId);
        item.ProductName.Should().Be("Fiber Optic");
        item.Quantity.Should().Be(2);
        item.UnitPrice.Should().Be(79.99m);

        order.SubTotal.Should().Be(79.99m * 2);
        order.DiscountTotal.Should().Be(10m);
        order.TaxTotal.Should().Be(15m);
        order.GrandTotal.Should().Be((79.99m * 2) + 39.99m - 10m + 15m);
    }

    [Fact]
    public void AddItem_WhenOrderNotDraft_ShouldThrowInvalidOperationException()
    {
        var order = CreateOrderWithItems();
        order.Submit();

        var act = () => order.AddItem(
            Guid.NewGuid(), Guid.NewGuid(),
            "Extra", "Extra Item",
            1, 10m, 0, 0, 0,
            BillingPeriod.Monthly);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*non-draft*");
    }

    [Fact]
    public void RemoveItem_ShouldRemoveItemAndRecalculate()
    {
        var order = CreateDraftOrder();
        order.AddItem(
            Guid.NewGuid(), Guid.NewGuid(),
            "Item A", "Offer A",
            1, 100m, 0, 0, 0,
            BillingPeriod.Monthly);
        var itemId = order.Items.First().Id;
        order.AddItem(
            Guid.NewGuid(), Guid.NewGuid(),
            "Item B", "Offer B",
            1, 200m, 0, 0, 0,
            BillingPeriod.Monthly);

        order.RemoveItem(itemId);

        order.Items.Should().HaveCount(1);
        order.Items.Single().ProductName.Should().Be("Item B");
        order.SubTotal.Should().Be(200m);
    }

    [Fact]
    public void RemoveItem_WhenItemNotFound_ShouldThrow()
    {
        var order = CreateDraftOrder();

        var act = () => order.RemoveItem(Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public void RemoveItem_WhenOrderNotDraft_ShouldThrow()
    {
        var order = CreateOrderWithItems();
        order.Submit();
        var itemId = order.Items.First().Id;

        var act = () => order.RemoveItem(itemId);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*non-draft*");
    }

    [Fact]
    public void Submit_ShouldChangeStatusToSubmitted()
    {
        var order = CreateOrderWithItems();

        order.Submit();

        order.Status.Should().Be(OrderStatus.Submitted);
    }

    [Fact]
    public void Submit_WhenNotDraft_ShouldThrow()
    {
        var order = CreateOrderWithItems();
        order.Submit();

        var act = () => order.Submit();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot submit order in Submitted status*");
    }

    [Fact]
    public void Submit_WithNoItems_ShouldThrow()
    {
        var order = CreateDraftOrder();

        var act = () => order.Submit();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*no items*");
    }

    [Fact]
    public void Approve_ShouldChangeStatusToApproved()
    {
        var order = CreateOrderWithItems();
        order.Submit();

        order.Approve("approver-1");

        order.Status.Should().Be(OrderStatus.Approved);
        order.ApprovedById.Should().Be("approver-1");
        order.ApprovedAt.Should().NotBeNull();
    }

    [Fact]
    public void Approve_WhenNotSubmitted_ShouldThrow()
    {
        var order = CreateDraftOrder();

        var act = () => order.Approve("user-1");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*approve*Draft*");
    }

    [Fact]
    public void Reject_ShouldChangeStatusToRejected()
    {
        var order = CreateOrderWithItems();
        order.Submit();

        order.Reject("approver-1", "Incomplete documentation");

        order.Status.Should().Be(OrderStatus.Rejected);
        order.CancellationReason.Should().Be("Incomplete documentation");
        order.ApprovedById.Should().Be("approver-1");
    }

    [Fact]
    public void Cancel_ShouldChangeStatusToCancelled()
    {
        var order = CreateOrderWithItems();
        order.Submit();

        order.Cancel("Customer request");

        order.Status.Should().Be(OrderStatus.Cancelled);
        order.CancellationReason.Should().Be("Customer request");
    }

    [Fact]
    public void Cancel_WhenCompleted_ShouldThrow()
    {
        var order = CreateOrderWithItems();
        order.Submit();
        order.Approve("approver-1");
        order.StartFulfillment();
        order.MarkCompleted();

        var act = () => order.Cancel("Too late");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*cancel*Completed*");
    }

    [Fact]
    public void MarkCompleted_ShouldChangeStatusToCompleted()
    {
        var order = CreateOrderWithItems();
        order.Submit();
        order.Approve("approver-1");
        order.StartFulfillment();

        order.MarkCompleted();

        order.Status.Should().Be(OrderStatus.Completed);
    }

    [Fact]
    public void MarkCompleted_WhenNotFulfillingOrApproved_ShouldThrow()
    {
        var order = CreateDraftOrder();

        var act = () => order.MarkCompleted();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*complete*Draft*");
    }

    [Fact]
    public void StartFulfillment_ShouldChangeStatusToFulfilling()
    {
        var order = CreateOrderWithItems();
        order.Submit();
        order.Approve("approver-1");

        order.StartFulfillment();

        order.Status.Should().Be(OrderStatus.Fulfilling);
    }

    [Fact]
    public void AddPayment_ShouldAddPaymentToCollection()
    {
        var order = CreateDraftOrder();

        order.AddPayment(100m, "CreditCard", "REF-001");

        order.Payments.Should().HaveCount(1);
        var payment = order.Payments.Single();
        payment.Amount.Should().Be(100m);
        payment.PaymentMethod.Should().Be("CreditCard");
        payment.PaymentReference.Should().Be("REF-001");
    }

    [Fact]
    public void MarkPaymentCompleted_ShouldUpdatePaymentStatus()
    {
        var order = CreateDraftOrder();
        order.AddPayment(100m, "CreditCard", "REF-002");
        var paymentId = order.Payments.First().Id;

        order.MarkPaymentCompleted(paymentId);

        order.Payments.Single().Status.Should().Be(PaymentStatus.Completed);
    }

    [Fact]
    public void Submit_ShouldRaiseProductOrderSubmittedDomainEvent()
    {
        var order = CreateOrderWithItems();

        order.Submit();

        order.DomainEvents.Should().ContainSingle(e => e is ProductOrderSubmittedDomainEvent);
        var domainEvent = order.DomainEvents.OfType<ProductOrderSubmittedDomainEvent>().Single();
        domainEvent.OrderId.Should().Be(order.Id);
        domainEvent.OrderNumber.Should().Be(order.OrderNumber);
        domainEvent.OrderItems.Should().HaveCount(1);
    }

    [Fact]
    public void Approve_ShouldRaiseProductOrderApprovedDomainEvent()
    {
        var order = CreateOrderWithItems();
        order.Submit();
        order.ClearDomainEvents();

        order.Approve("approver-1");

        order.DomainEvents.Should().ContainSingle(e => e is ProductOrderApprovedDomainEvent);
        var domainEvent = order.DomainEvents.OfType<ProductOrderApprovedDomainEvent>().Single();
        domainEvent.OrderId.Should().Be(order.Id);
        domainEvent.ApprovedBy.Should().Be("approver-1");
    }

    [Fact]
    public void Cancel_ShouldRaiseProductOrderCancelledDomainEvent()
    {
        var order = CreateOrderWithItems();
        order.Submit();
        order.ClearDomainEvents();

        order.Cancel("Customer request");

        order.DomainEvents.Should().ContainSingle(e => e is ProductOrderCancelledDomainEvent);
        var domainEvent = order.DomainEvents.OfType<ProductOrderCancelledDomainEvent>().Single();
        domainEvent.OrderId.Should().Be(order.Id);
        domainEvent.Reason.Should().Be("Customer request");
    }

    [Fact]
    public void MarkCompleted_ShouldRaiseProductOrderCompletedDomainEvent()
    {
        var order = CreateOrderWithItems();
        order.Submit();
        order.Approve("approver-1");
        order.StartFulfillment();
        order.ClearDomainEvents();

        order.MarkCompleted();

        order.DomainEvents.Should().ContainSingle(e => e is ProductOrderCompletedDomainEvent);
    }

    [Fact]
    public void OrderNumber_ShouldBeUniqueEachTime()
    {
        var order1 = CreateDraftOrder();
        var order2 = CreateDraftOrder();

        order1.OrderNumber.Should().NotBe(order2.OrderNumber);
    }

    [Fact]
    public void CalculateTotals_WithMultipleItems_ShouldComputeCorrectly()
    {
        var order = CreateDraftOrder();

        order.AddItem(Guid.NewGuid(), Guid.NewGuid(), "Item1", "Offer1",
            2, 50m, 10m, 5m, 8m, BillingPeriod.Monthly);
        order.AddItem(Guid.NewGuid(), Guid.NewGuid(), "Item2", "Offer2",
            1, 100m, 20m, 10m, 15m, BillingPeriod.Annual);

        order.SubTotal.Should().Be(200m);
        order.DiscountTotal.Should().Be(15m);
        order.TaxTotal.Should().Be(23m);
        order.GrandTotal.Should().Be(200m + 30m - 15m + 23m);
    }
}
