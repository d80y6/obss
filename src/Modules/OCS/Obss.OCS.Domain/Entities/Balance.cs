using Obss.OCS.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.OCS.Domain.Entities;

public class Balance : AggregateRoot<Guid>, ITenantEntity
{
    private Balance() { }

    private Balance(Guid id, string tenantId, Guid subscriptionId, string currency)
        : base(id)
    {
        TenantId = tenantId;
        SubscriptionId = subscriptionId;
        Currency = currency;
        AvailableAmount = 0;
        ReservedAmount = 0;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public string TenantId { get; private set; } = string.Empty;
    public Guid SubscriptionId { get; private set; }
    public decimal AvailableAmount { get; private set; }
    public decimal ReservedAmount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public static Balance Create(string tenantId, Guid subscriptionId, string currency)
    {
        return new Balance(Guid.NewGuid(), tenantId, subscriptionId, currency);
    }

    public void Credit(decimal amount)
    {
        AvailableAmount += amount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Debit(decimal amount)
    {
        if (AvailableAmount - ReservedAmount < amount)
            throw new Exceptions.InsufficientBalanceException($"Insufficient available balance: {AvailableAmount - ReservedAmount} < {amount}");
        AvailableAmount -= amount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reserve(decimal amount)
    {
        if (AvailableAmount - ReservedAmount < amount)
            throw new Exceptions.InsufficientBalanceException($"Insufficient balance to reserve {amount}");
        ReservedAmount += amount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReleaseReservation(decimal amount)
    {
        ReservedAmount = Math.Max(0, ReservedAmount - amount);
        UpdatedAt = DateTime.UtcNow;
    }

    public decimal EffectiveBalance => AvailableAmount - ReservedAmount;
}
