using Obss.Orders.Domain.Events;
using Obss.Orders.Domain.Exceptions;
using Obss.Orders.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.Orders.Domain.Entities;

public sealed record RelatedParty(string PartyId, string PartyName, string Role);

public class ProductOrder : AggregateRoot<Guid>
{
    private readonly List<ProductOrderItem> _items = [];
    private readonly List<ProductOrderPayment> _payments = [];
    private readonly List<RelatedParty> _relatedParties = [];
    private readonly List<ProductOrderMilestone> _milestones = [];
    private readonly List<ProductOrderItemRelationship> _itemRelationships = [];

    private ProductOrder() { }

    private ProductOrder(
        Guid id,
        string tenantId,
        Guid customerId,
        string customerName,
        OrderType orderType,
        string? notes,
        Address? billingAddress,
        Address? shippingAddress,
        string createdById,
        string currency,
        Guid? billingAccountId,
        Priority? orderPriority,
        string? billingAccountHref = null,
        Guid? productOfferingQualificationId = null,
        string? productOfferingQualificationHref = null,
        string? quoteHref = null)
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
        BillingAccountId = billingAccountId;
        OrderPriority = orderPriority ?? Priority.Medium;
        BillingAccountHref = billingAccountHref;
        ProductOfferingQualificationId = productOfferingQualificationId;
        ProductOfferingQualificationHref = productOfferingQualificationHref;
        QuoteHref = quoteHref;

        _milestones.Add(new ProductOrderMilestone(Id, "OrderCreated", "Order was created", DateTime.UtcNow));
        _milestones[^1].Achieve();
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
    public Priority OrderPriority { get; private set; } = Priority.Medium;
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
    public Guid? BillingAccountId { get; private set; }
#pragma warning disable S1144 // Used by EF Core via reflection
    public string? BillingAccountHref { get; private set; }
#pragma warning restore S1144
    public int OrderVersion { get; private set; } = 1;
#pragma warning disable S1144 // Used by EF Core via reflection
    public Guid? ProductOfferingQualificationId { get; private set; }
    public string? ProductOfferingQualificationHref { get; private set; }
    public string? QuoteHref { get; private set; }
#pragma warning restore S1144

    public IReadOnlyCollection<ProductOrderItem> Items => _items.AsReadOnly();
    public IReadOnlyCollection<ProductOrderPayment> Payments => _payments.AsReadOnly();
    public IReadOnlyCollection<RelatedParty> RelatedParties => _relatedParties.AsReadOnly();
    public IReadOnlyList<ProductOrderMilestone> Milestones => _milestones.AsReadOnly();
    public IReadOnlyList<ProductOrderItemRelationship> ItemRelationships => _itemRelationships.AsReadOnly();
#pragma warning disable S1144, S2933, S3459, CS0649 // Used by EF Core via reflection
    private OrderFulfillment? _fulfillment;
#pragma warning restore S1144, S2933, S3459, CS0649
    public OrderFulfillment? Fulfillment => _fulfillment;

    public static ProductOrder Create(
        string tenantId,
        Guid customerId,
        string customerName,
        OrderType orderType,
        string createdById,
        string? notes = null,
        Address? billingAddress = null,
        Address? shippingAddress = null,
        string currency = "USD",
        Priority? priority = null,
        Guid? billingAccountId = null,
        string? billingAccountHref = null,
        Guid? productOfferingQualificationId = null,
        string? productOfferingQualificationHref = null,
        string? quoteHref = null)
    {
        return new ProductOrder(
            Guid.NewGuid(),
            tenantId,
            customerId,
            customerName,
            orderType,
            notes,
            billingAddress,
            shippingAddress,
            createdById,
            currency,
            billingAccountId,
            priority,
            billingAccountHref,
            productOfferingQualificationId,
            productOfferingQualificationHref,
            quoteHref);
    }

    public void Submit()
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException($"Cannot submit order in {Status} status.");

        if (_items.Count == 0)
            throw new InvalidOperationException("Cannot submit an order with no items.");

        Status = OrderStatus.Submitted;

        _milestones.Add(new ProductOrderMilestone(Id, "OrderSubmitted", "Order was submitted", DateTime.UtcNow));
        _milestones[^1].Achieve();

        AddDomainEvent(new ProductOrderSubmittedDomainEvent(
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

        _milestones.Add(new ProductOrderMilestone(Id, "OrderApproved", "Order was approved", DateTime.UtcNow));
        _milestones[^1].Achieve();

        AddDomainEvent(new ProductOrderApprovedDomainEvent(Id, OrderNumber, userId));
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

        _milestones.Add(new ProductOrderMilestone(Id, "FulfillmentStarted", "Fulfillment process started", DateTime.UtcNow));
        _milestones[^1].Achieve();
    }

    public void Reject(string userId, string reason)
    {
        if (Status != OrderStatus.Submitted && Status != OrderStatus.PendingApproval)
            throw new InvalidOperationException($"Cannot reject order in {Status} status.");

        Status = OrderStatus.Rejected;
        ApprovedById = userId;
        ApprovedAt = DateTime.UtcNow;
        CancellationReason = reason;

        AddDomainEvent(new ProductOrderCancelledDomainEvent(Id, reason));
    }

    public void Cancel(string reason)
    {
        if (Status == OrderStatus.Completed || Status == OrderStatus.Cancelled)
            throw new InvalidOperationException($"Cannot cancel order in {Status} status.");

        Status = OrderStatus.Cancelled;
        CancellationReason = reason;

        AddDomainEvent(new ProductOrderCancelledDomainEvent(Id, reason));
    }

    public void MarkCompleted()
    {
        if (Status != OrderStatus.Fulfilling && Status != OrderStatus.Approved)
            throw new InvalidOperationException($"Cannot complete order in {Status} status.");

        Status = OrderStatus.Completed;

        _milestones.Add(new ProductOrderMilestone(Id, "OrderCompleted", "Order was completed", DateTime.UtcNow));
        _milestones[^1].Achieve();

        AddDomainEvent(new ProductOrderCompletedDomainEvent(Id));
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

        var item = new ProductOrderItem(
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
        var payment = new ProductOrderPayment(
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
        Priority? priority = null,
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
        if (priority.HasValue) OrderPriority = priority.Value;
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

    public void AddItemRelationship(Guid itemId, Guid targetItemId, RelationshipType type)
    {
        if (HasCircularDependency(itemId, targetItemId))
            throw new InvalidProductOrderStateException("Circular dependency detected in item relationships");
        _itemRelationships.Add(new ProductOrderItemRelationship(itemId, targetItemId, type));
    }

    public void RemoveItemRelationship(Guid relationshipId)
    {
        var rel = _itemRelationships.FirstOrDefault(r => r.Id == relationshipId);
        if (rel is null) throw new InvalidProductOrderStateException($"Relationship {relationshipId} not found");
        _itemRelationships.Remove(rel);
    }

    private bool HasCircularDependency(Guid itemId, Guid targetItemId)
    {
        var visited = new HashSet<Guid> { itemId };
        var queue = new Queue<Guid>([targetItemId]);
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current == itemId) return true;
            if (!visited.Add(current)) continue;
            foreach (var rel in _itemRelationships.Where(r => r.ProductOrderItemId == current))
                queue.Enqueue(rel.TargetItemId);
        }
        return false;
    }

    public IReadOnlyList<ProductOrderItemRelationship> GetItemRelationships(Guid itemId) =>
        _itemRelationships.Where(r => r.ProductOrderItemId == itemId).ToList().AsReadOnly();

    public void AddMilestone(ProductOrderMilestone milestone)
    {
        _milestones.Add(milestone);
    }

    public void RemoveMilestone(Guid milestoneId)
    {
        var milestone = _milestones.FirstOrDefault(m => m.Id == milestoneId);
        if (milestone is null) throw new InvalidProductOrderStateException($"Milestone {milestoneId} not found");
        _milestones.Remove(milestone);
    }

    private static string GenerateOrderNumber()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var random = Random.Shared.Next(1000, 9999);
        return $"ORD-{timestamp}-{random}";
    }
}
