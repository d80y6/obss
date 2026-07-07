using Obss.SharedKernel.Domain.Common;
using Obss.Subscriptions.Domain.Events;
using Obss.Subscriptions.Domain.ValueObjects;

namespace Obss.Subscriptions.Domain.Entities;

public class SubscriptionEntitlement : AggregateRoot<Guid>
{
    private SubscriptionEntitlement() { }

    private SubscriptionEntitlement(
        Guid id,
        Guid subscriptionId,
        EntitlementType entitlementType,
        string name,
        decimal limit,
        decimal used,
        string unit,
        bool isUnlimited,
        bool isOverridable,
        DateTime validFrom,
        DateTime? validTo)
        : base(id)
    {
        SubscriptionId = subscriptionId;
        EntitlementType = entitlementType;
        Name = name;
        Limit = limit;
        Used = used;
        Unit = unit;
        IsUnlimited = isUnlimited;
        IsOverridable = isOverridable;
        ValidFrom = validFrom;
        ValidTo = validTo;
    }

    public Guid SubscriptionId { get; private set; }
    public EntitlementType EntitlementType { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public decimal Limit { get; private set; }
    public decimal Used { get; private set; }
    public string Unit { get; private set; } = string.Empty;
    public bool IsUnlimited { get; private set; }
    public bool IsOverridable { get; private set; }
    public DateTime ValidFrom { get; private set; }
    public DateTime? ValidTo { get; private set; }

    public Subscription Subscription { get; private set; } = null!;

    public static SubscriptionEntitlement Create(
        Guid subscriptionId,
        EntitlementType entitlementType,
        string name,
        decimal limit,
        decimal used,
        string unit,
        bool isUnlimited,
        bool isOverridable,
        DateTime validFrom,
        DateTime? validTo = null)
    {
        return new SubscriptionEntitlement(
            Guid.NewGuid(),
            subscriptionId,
            entitlementType,
            name,
            limit,
            used,
            unit,
            isUnlimited,
            isOverridable,
            validFrom,
            validTo);
    }

    public void UpdateLimit(decimal newLimit)
    {
        if (newLimit < 0)
            throw new ArgumentException("Limit must be non-negative.", nameof(newLimit));

        Limit = newLimit;
        IsUnlimited = false;

        AddDomainEvent(new EntitlementUpdatedDomainEvent(
            SubscriptionId, EntitlementType.ToString(), Used, Used));
    }

    public void RecordUsage(decimal amount)
    {
        if (amount < 0)
            throw new ArgumentException("Usage amount must be non-negative.", nameof(amount));

        var previousUsed = Used;
        Used += amount;

        AddDomainEvent(new EntitlementUpdatedDomainEvent(
            SubscriptionId, EntitlementType.ToString(), previousUsed, Used));

        if (!IsUnlimited && Used >= Limit)
        {
            AddDomainEvent(new EntitlementLimitReachedDomainEvent(
                SubscriptionId, EntitlementType.ToString(), Used, Limit));
        }
    }

    public void ReduceUsage(decimal amount)
    {
        if (amount < 0)
            throw new ArgumentException("Reduction amount must be non-negative.", nameof(amount));

        var previousUsed = Used;
        Used = Math.Max(0, Used - amount);

        AddDomainEvent(new EntitlementUpdatedDomainEvent(
            SubscriptionId, EntitlementType.ToString(), previousUsed, Used));
    }

    public bool IsAvailable(decimal requested)
    {
        if (requested < 0)
            throw new ArgumentException("Requested amount must be non-negative.", nameof(requested));

        if (IsUnlimited)
            return true;

        if (ValidTo.HasValue && ValidTo.Value <= DateTime.UtcNow)
            return false;

        return Used + requested <= Limit;
    }
}
