using Obss.SharedKernel.Domain.Common;
using Obss.SharedKernel.Infrastructure.Persistence;
using Obss.Subscriptions.Domain.Events;
using Obss.Subscriptions.Domain.Exceptions;
using Obss.Subscriptions.Domain.ValueObjects;

namespace Obss.Subscriptions.Domain.Entities;

public sealed record RelatedParty(string PartyId, string PartyName, string Role);

public class Subscription : AggregateRoot<Guid>, ITenantEntity
{
    private readonly List<SubscriptionService> _services = [];
    private readonly List<SubscriptionAddOn> _addOns = [];
    private readonly List<RelatedParty> _relatedParties = [];

    private Subscription() { }

    private Subscription(
        Guid id,
        string tenantId,
        Guid customerId,
        string customerName,
        Guid orderId,
        Guid orderItemId,
        Guid productId,
        Guid offerId,
        string offerName,
        BillingPeriod billingPeriod,
        string currency,
        decimal price,
        int quantity,
        DateTime startDate,
        DateTime? endDate)
        : base(id)
    {
        TenantId = tenantId;
        CustomerId = customerId;
        CustomerName = customerName;
        OrderId = orderId;
        OrderItemId = orderItemId;
        ProductId = productId;
        OfferId = offerId;
        OfferName = offerName;
        Status = SubscriptionStatus.Pending;
        BillingPeriod = billingPeriod;
        Currency = currency;
        Price = price;
        Quantity = quantity;
        StartDate = startDate;
        EndDate = endDate;
        RenewalDate = CalculateRenewalDate(startDate, billingPeriod);
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public string TenantId { get; private set; } = string.Empty;
    public Guid CustomerId { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public Guid OrderId { get; private set; }
    public Guid OrderItemId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid OfferId { get; private set; }
    public string OfferName { get; private set; } = string.Empty;
    public SubscriptionStatus Status { get; private set; }
    public BillingPeriod BillingPeriod { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int Quantity { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public DateTime? SuspendedAt { get; private set; }
    public DateTime? ActivationDate { get; private set; }
    public DateTime? RenewalDate { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public string? Href { get; private set; }
    public string? AtType { get; private set; } = "Subscription";
    public string? AtBaseType { get; private set; } = "ProductSubscription";
    public string? AtSchemaLocation { get; private set; }
    public string? Name { get; private set; }
    public string? Description { get; private set; }
    public string? ExternalId { get; private set; }
    public string? CompletionDate { get; private set; }

    public IReadOnlyCollection<SubscriptionService> Services => _services.AsReadOnly();
    public IReadOnlyCollection<SubscriptionAddOn> AddOns => _addOns.AsReadOnly();
    public IReadOnlyCollection<RelatedParty> RelatedParties => _relatedParties.AsReadOnly();

    public static Subscription Create(
        string tenantId,
        Guid customerId,
        string customerName,
        Guid orderId,
        Guid orderItemId,
        Guid productId,
        Guid offerId,
        string offerName,
        BillingPeriod billingPeriod,
        string currency,
        decimal price,
        int quantity,
        DateTime startDate,
        DateTime? endDate = null)
    {
        return new Subscription(
            Guid.NewGuid(),
            tenantId,
            customerId,
            customerName,
            orderId,
            orderItemId,
            productId,
            offerId,
            offerName,
            billingPeriod,
            currency,
            price,
            quantity,
            startDate,
            endDate);
    }

    public void Activate()
    {
        if (Status != SubscriptionStatus.Pending)
            throw new InvalidSubscriptionStateException(
                $"Cannot activate subscription in '{Status}' status. Only 'Pending' subscriptions can be activated.");

        Status = SubscriptionStatus.Active;
        ActivationDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new SubscriptionActivatedDomainEvent(
            Id, CustomerId, OfferId, ActivationDate.Value));
    }

    public void Suspend(string reason)
    {
        if (Status != SubscriptionStatus.Active)
            throw new InvalidSubscriptionStateException(
                $"Cannot suspend subscription in '{Status}' status. Only 'Active' subscriptions can be suspended.");

        Status = SubscriptionStatus.Suspended;
        SuspendedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new SubscriptionSuspendedDomainEvent(Id, reason));
    }

    public void Resume()
    {
        if (Status != SubscriptionStatus.Suspended)
            throw new InvalidSubscriptionStateException(
                $"Cannot resume subscription in '{Status}' status. Only 'Suspended' subscriptions can be resumed.");

        Status = SubscriptionStatus.Active;
        SuspendedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel(string reason, DateTime effectiveDate)
    {
        if (Status is SubscriptionStatus.Cancelled or SubscriptionStatus.Expired)
            throw new InvalidSubscriptionStateException(
                $"Cannot cancel subscription in '{Status}' status.");

        Status = SubscriptionStatus.Cancelled;
        CancelledAt = effectiveDate;
        EndDate = effectiveDate;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new SubscriptionCancelledDomainEvent(Id, CustomerId, effectiveDate));
    }

    public void Renew()
    {
        if (Status != SubscriptionStatus.Active)
            throw new InvalidSubscriptionStateException(
                $"Cannot renew subscription in '{Status}' status. Only 'Active' subscriptions can be renewed.");

        RenewalDate = CalculateRenewalDate(DateTime.UtcNow, BillingPeriod);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new SubscriptionRenewedDomainEvent(Id, RenewalDate.Value, Price));
    }

    public void ChangeOffer(Guid newOfferId, decimal newPrice)
    {
        if (Status != SubscriptionStatus.Active)
            throw new InvalidSubscriptionStateException(
                $"Cannot change offer for subscription in '{Status}' status.");

        OfferId = newOfferId;
        Price = newPrice;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeQuantity(int newQuantity)
    {
        if (newQuantity < 1)
            throw new ArgumentException("Quantity must be at least 1.", nameof(newQuantity));

        if (Status != SubscriptionStatus.Active && Status != SubscriptionStatus.Suspended)
            throw new InvalidSubscriptionStateException(
                $"Cannot change quantity for subscription in '{Status}' status.");

        Quantity = newQuantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ExtendEndDate(DateTime newEndDate)
    {
        if (newEndDate <= (EndDate ?? StartDate))
            throw new ArgumentException("New end date must be after current end date.", nameof(newEndDate));

        EndDate = newEndDate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddService(SubscriptionService service)
    {
        _services.Add(service);
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddAddOn(SubscriptionAddOn addOn)
    {
        _addOns.Add(addOn);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveAddOn(Guid addOnId)
    {
        var addOn = _addOns.FirstOrDefault(a => a.Id == addOnId);
        if (addOn is not null)
        {
            addOn.Deactivate();
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void UpdateTmfDetails(
        string? name = null,
        string? description = null,
        string? externalId = null,
        string? completionDate = null,
        string? href = null,
        string? atSchemaLocation = null)
    {
        if (name is not null) Name = name;
        if (description is not null) Description = description;
        if (externalId is not null) ExternalId = externalId;
        if (completionDate is not null) CompletionDate = completionDate;
        if (href is not null) Href = href;
        if (atSchemaLocation is not null) AtSchemaLocation = atSchemaLocation;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddRelatedParty(RelatedParty party)
    {
        _relatedParties.Add(party);
        UpdatedAt = DateTime.UtcNow;
    }

    private static DateTime CalculateRenewalDate(DateTime fromDate, BillingPeriod billingPeriod)
    {
        return billingPeriod switch
        {
            ValueObjects.BillingPeriod.Monthly => fromDate.AddMonths(1),
            ValueObjects.BillingPeriod.Quarterly => fromDate.AddMonths(3),
            ValueObjects.BillingPeriod.SemiAnnual => fromDate.AddMonths(6),
            ValueObjects.BillingPeriod.Annual => fromDate.AddYears(1),
            _ => fromDate.AddMonths(1)
        };
    }
}
