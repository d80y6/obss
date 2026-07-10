using Obss.Orders.Domain.Events;
using Obss.Orders.Domain.Exceptions;
using Obss.Orders.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Orders.Domain.Entities;

public class ProductOrderItem : Entity<Guid>
{
    internal ProductOrderItem(
        Guid id,
        Guid orderId,
        Guid productId,
        Guid offerId,
        string productName,
        string offerName,
        int quantity,
        decimal unitPrice,
        decimal recurringPrice,
        decimal discountAmount,
        decimal taxAmount,
        decimal totalPrice,
        BillingPeriod billingPeriod,
        DateTime startDate,
        DateTime? endDate,
        bool isActive,
        string? serviceType = null)
        : base(id)
    {
        OrderId = orderId;
        ProductId = productId;
        OfferId = offerId;
        ProductName = productName;
        OfferName = offerName;
        Quantity = quantity;
        UnitPrice = unitPrice;
        RecurringPrice = recurringPrice;
        DiscountAmount = discountAmount;
        TaxAmount = taxAmount;
        TotalPrice = totalPrice;
        BillingPeriod = billingPeriod;
        StartDate = startDate;
        EndDate = endDate;
        IsActive = isActive;
        ServiceType = serviceType;
    }

    private ProductOrderItem() { }

    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid OfferId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public string OfferName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal RecurringPrice { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalPrice { get; private set; }
    public BillingPeriod BillingPeriod { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public bool IsActive { get; private set; }
    public string? ServiceType { get; private set; }
#pragma warning disable S1144 // Used by EF Core via reflection
    public string? Action { get; private set; }
    public string? ItemState { get; private set; }
#pragma warning restore S1144
    public ProductOrderItemState State { get; private set; } = ProductOrderItemState.Acknowledged;

    public void Acknowledge()
    {
        if (State is not ProductOrderItemState.Acknowledged)
            throw new InvalidProductOrderItemStateException($"Cannot acknowledge from {State}");
        TransitionTo(ProductOrderItemState.Acknowledged);
    }

    public void StartProgress()
    {
        if (State != ProductOrderItemState.Acknowledged && State != ProductOrderItemState.Pending)
            throw new InvalidProductOrderItemStateException($"Cannot start progress from {State}");
        TransitionTo(ProductOrderItemState.InProgress);
    }

    public void Hold()
    {
        if (State != ProductOrderItemState.InProgress)
            throw new InvalidProductOrderItemStateException($"Cannot hold from {State}");
        TransitionTo(ProductOrderItemState.Held);
    }

    public void Resume()
    {
        if (State != ProductOrderItemState.Held)
            throw new InvalidProductOrderItemStateException($"Cannot resume from {State}");
        TransitionTo(ProductOrderItemState.InProgress);
    }

    public void Assess()
    {
        if (State != ProductOrderItemState.InProgress)
            throw new InvalidProductOrderItemStateException($"Cannot assess from {State}");
        TransitionTo(ProductOrderItemState.Assessing);
    }

    public void Pending(string reason)
    {
        if (State != ProductOrderItemState.InProgress)
            throw new InvalidProductOrderItemStateException($"Cannot set pending from {State}");
        TransitionTo(ProductOrderItemState.Pending, reason);
    }

    public void Reject(string reason)
    {
        if (State != ProductOrderItemState.Assessing)
            throw new InvalidProductOrderItemStateException($"Cannot reject from {State}");
        TransitionTo(ProductOrderItemState.Rejected, reason);
    }

    public void Complete()
    {
        if (State != ProductOrderItemState.InProgress && State != ProductOrderItemState.Assessing)
            throw new InvalidProductOrderItemStateException($"Cannot complete from {State}");
        TransitionTo(ProductOrderItemState.Completed);
    }

    public void Fail(string error)
    {
        if (State != ProductOrderItemState.InProgress)
            throw new InvalidProductOrderItemStateException($"Cannot fail from {State}");
        TransitionTo(ProductOrderItemState.Failed, error);
    }

    public void Cancel()
    {
        if (State is ProductOrderItemState.Completed or ProductOrderItemState.Failed or ProductOrderItemState.Rejected or ProductOrderItemState.Cancelled)
            throw new InvalidProductOrderItemStateException($"Cannot cancel from {State}");
        TransitionTo(ProductOrderItemState.Cancelled);
    }

    private void TransitionTo(ProductOrderItemState newState, string? reason = null)
    {
        var oldState = State;
        State = newState;
        AddDomainEvent(new ProductOrderItemStateChangedDomainEvent(OrderId, Id, oldState, newState, reason));
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void UpdateQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        Quantity = quantity;
    }
}
