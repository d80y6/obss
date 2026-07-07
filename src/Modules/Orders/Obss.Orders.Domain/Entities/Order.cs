using Obss.Orders.Domain.Events;
using Obss.Orders.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.Orders.Domain.Entities;

public sealed record RelatedParty(string PartyId, string PartyName, string Role);

public class Order : AggregateRoot<Guid>
{
    private readonly List<OrderItem> _items = [];
    private readonly List<OrderPayment> _payments = [];
    private readonly List<RelatedParty> _relatedParties = [];

    private Order() { }

    private Order(
        Guid id,
        string tenantId,
        Guid customerId,
        string customerName,
        OrderType orderType,
        string? notes,
        Address? billingAddress,
        Address? shippingAddress,
        string createdById,
        string currency)
        : base(id)
    {
        TenantId = tenantId;
        OrderNumber = GenerateOrderNumber();
        CustomerId = customerId;
        CustomerName = customerName;
        OrderDate = DateTime.UtcNow;
        Status = OrderStatus.Draft;
        OrderType = orderType;
        Notes = notes;
        BillingAddress = billingAddress;
        ShippingAddress = shippingAddress;
        CreatedById = createdById;
        Currency = currency;
        SubTotal = 0;
        TaxTotal = 0;
        DiscountTotal = 0;
        GrandTotal = 0;
    }

    public string TenantId { get; private set; } = string.Empty;
    public string OrderNumber { get; private set; } = string.Empty;
    public Guid CustomerId { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public DateTime OrderDate { get; private set; }
    public OrderStatus Status { get; private set; }
    public OrderType OrderType { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal TaxTotal { get; private set; }
    public decimal DiscountTotal { get; private set; }
    public decimal GrandTotal { get; private set; }
    public string Currency { get; private set; } = "USD";
    public string? Notes { get; private set; }
    public Address? BillingAddress { get; private set; }
    public Address? ShippingAddress { get; private set; }
    public string CreatedById { get; private set; } = string.Empty;
    public string? ApprovedById { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public string? CancellationReason { get; private set; }
    public string? Description { get; private set; }
    public string? Channel { get; private set; }
    public string? Priority { get; private set; }
    public DateTime? RequestedStartDate { get; private set; }
    public DateTime? RequestedCompletionDate { get; private set; }
    public DateTime? ExpectedCompletionDate { get; private set; }
    public string? NotificationContact { get; private set; }
    public string? ExternalId { get; private set; }
    public Guid? QuoteId { get; private set; }
    public string? Href { get; private set; }
    public string? AtType { get; private set; } = "Order";
    public string? AtBaseType { get; private set; } = "ProductOrder";
#pragma warning disable S1144 // Used by EF Core via reflection
    public string? AtSchemaLocation { get; private set; }
#pragma warning restore S1144
    public string? CompletionDate { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    public IReadOnlyCollection<OrderPayment> Payments => _payments.AsReadOnly();
    public IReadOnlyCollection<RelatedParty> RelatedParties => _relatedParties.AsReadOnly();
#pragma warning disable S1144, S2933, S3459, CS0649 // Used by EF Core via reflection
    private OrderFulfillment? _fulfillment;
#pragma warning restore S1144, S2933, S3459, CS0649
    public OrderFulfillment? Fulfillment => _fulfillment;

    public static Order Create(
        string tenantId,
        Guid customerId,
        string customerName,
        OrderType orderType,
        string createdById,
        string? notes = null,
        Address? billingAddress = null,
        Address? shippingAddress = null,
        string currency = "USD")
    {
        return new Order(
            Guid.NewGuid(),
            tenantId,
            customerId,
            customerName,
            orderType,
            notes,
            billingAddress,
            shippingAddress,
            createdById,
            currency);
    }

    public void Submit()
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException($"Cannot submit order in {Status} status.");

        if (_items.Count == 0)
            throw new InvalidOperationException("Cannot submit an order with no items.");

        Status = OrderStatus.Submitted;

        AddDomainEvent(new OrderSubmittedDomainEvent(
            Id,
            OrderNumber,
            CustomerId,
            GrandTotal,
            Currency,
            _items.Select(i => new OrderItemData(
                i.Id,
                i.ProductId,
                i.OfferId,
                i.ProductName,
                i.OfferName,
                i.Quantity,
                i.UnitPrice,
                i.TotalPrice)).ToList()));
    }

    public void Approve(string userId)
    {
        if (Status != OrderStatus.Submitted && Status != OrderStatus.PendingApproval)
            throw new InvalidOperationException($"Cannot approve order in {Status} status.");

        Status = OrderStatus.Approved;
        ApprovedById = userId;
        ApprovedAt = DateTime.UtcNow;

        AddDomainEvent(new OrderApprovedDomainEvent(Id, OrderNumber, userId));
    }

    public void StartFulfillment()
    {
        if (Status != OrderStatus.Approved && Status != OrderStatus.Fulfilling)
            throw new InvalidOperationException($"Cannot start fulfillment for order in {Status} status.");

        Status = OrderStatus.Fulfilling;
    }

    public void CreateFulfillment()
    {
        if (_fulfillment is not null)
            throw new InvalidOperationException("Fulfillment already exists for this order.");

        _fulfillment = OrderFulfillment.Create(Id);
        StartFulfillment();
    }

    public void Reject(string userId, string reason)
    {
        if (Status != OrderStatus.Submitted && Status != OrderStatus.PendingApproval)
            throw new InvalidOperationException($"Cannot reject order in {Status} status.");

        Status = OrderStatus.Rejected;
        ApprovedById = userId;
        ApprovedAt = DateTime.UtcNow;
        CancellationReason = reason;

        AddDomainEvent(new OrderCancelledDomainEvent(Id, reason));
    }

    public void Cancel(string reason)
    {
        if (Status == OrderStatus.Completed || Status == OrderStatus.Cancelled)
            throw new InvalidOperationException($"Cannot cancel order in {Status} status.");

        Status = OrderStatus.Cancelled;
        CancellationReason = reason;

        AddDomainEvent(new OrderCancelledDomainEvent(Id, reason));
    }

    public void MarkCompleted()
    {
        if (Status != OrderStatus.Fulfilling && Status != OrderStatus.Approved)
            throw new InvalidOperationException($"Cannot complete order in {Status} status.");

        Status = OrderStatus.Completed;

        AddDomainEvent(new OrderCompletedDomainEvent(Id));
    }

    public void AddItem(
        Guid productId,
        Guid offerId,
        string productName,
        string offerName,
        int quantity,
        decimal unitPrice,
        decimal recurringPrice,
        decimal discountAmount,
        decimal taxAmount,
        BillingPeriod billingPeriod,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? serviceType = null)
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException("Cannot add items to a non-draft order.");

        var totalPrice = (unitPrice * quantity) + recurringPrice - discountAmount + taxAmount;

        var item = new OrderItem(
            Guid.NewGuid(),
            Id,
            productId,
            offerId,
            productName,
            offerName,
            quantity,
            unitPrice,
            recurringPrice,
            discountAmount,
            taxAmount,
            totalPrice,
            billingPeriod,
            startDate ?? DateTime.UtcNow,
            endDate,
            true,
            serviceType);

        _items.Add(item);
        CalculateTotals();
    }

    public void RemoveItem(Guid itemId)
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException("Cannot remove items from a non-draft order.");

        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new InvalidOperationException($"Item with id {itemId} not found.");

        _items.Remove(item);
        CalculateTotals();
    }

    public void CalculateTotals()
    {
        SubTotal = _items.Sum(i => i.UnitPrice * i.Quantity);
        DiscountTotal = _items.Sum(i => i.DiscountAmount);
        TaxTotal = _items.Sum(i => i.TaxAmount);

        var recurringTotal = _items.Sum(i => i.RecurringPrice);
        GrandTotal = SubTotal + recurringTotal - DiscountTotal + TaxTotal;
    }

    public void AddPayment(decimal amount, string paymentMethod, string paymentReference)
    {
        var payment = new OrderPayment(
            Guid.NewGuid(),
            Id,
            amount,
            paymentMethod,
            paymentReference,
            DateTime.UtcNow,
            PaymentStatus.Pending);

        _payments.Add(payment);
    }

    public void MarkPaymentCompleted(Guid paymentId)
    {
        var payment = _payments.FirstOrDefault(p => p.Id == paymentId)
            ?? throw new InvalidOperationException($"Payment with id {paymentId} not found.");

        payment.Complete();
    }

    public void UpdateDetails(
        string? description = null,
        string? channel = null,
        string? priority = null,
        string? notes = null,
        DateTime? requestedStartDate = null,
        DateTime? requestedCompletionDate = null,
        DateTime? expectedCompletionDate = null,
        string? notificationContact = null,
        string? externalId = null,
        Guid? quoteId = null,
        Address? billingAddress = null,
        Address? shippingAddress = null)
    {
        if (Status != OrderStatus.Draft && Status != OrderStatus.Submitted)
            throw new InvalidOperationException($"Cannot update order in {Status} status.");

        if (description is not null) Description = description;
        if (channel is not null) Channel = channel;
        if (priority is not null) Priority = priority;
        if (notes is not null) Notes = notes;
        if (requestedStartDate.HasValue) RequestedStartDate = requestedStartDate;
        if (requestedCompletionDate.HasValue) RequestedCompletionDate = requestedCompletionDate;
        if (expectedCompletionDate.HasValue) ExpectedCompletionDate = expectedCompletionDate;
        if (notificationContact is not null) NotificationContact = notificationContact;
        if (externalId is not null) ExternalId = externalId;
        if (quoteId.HasValue) QuoteId = quoteId;
        if (billingAddress is not null) BillingAddress = billingAddress;
        if (shippingAddress is not null) ShippingAddress = shippingAddress;
    }

    public void SetHref(string href)
    {
        Href = href;
    }

    public void UpdateTmfDetails(string? completionDate = null, string? cancellationDate = null, string? cancelledBy = null)
    {
        if (completionDate is not null) CompletionDate = completionDate;
    }

    public void AddRelatedParty(string partyId, string partyName, string role)
    {
        _relatedParties.Add(new RelatedParty(partyId, partyName, role));
    }

    private static string GenerateOrderNumber()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var random = Random.Shared.Next(1000, 9999);
        return $"ORD-{timestamp}-{random}";
    }
}
