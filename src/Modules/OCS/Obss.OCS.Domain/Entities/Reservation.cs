using Obss.SharedKernel.Domain.Common;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.OCS.Domain.Entities;

public enum ReservationStatus
{
    Reserved,
    Debited,
    Released,
    Expired
}

public class Reservation : AggregateRoot<Guid>, ITenantEntity
{
    private Reservation() { }

    private Reservation(Guid id, string tenantId, Guid balanceId, Guid subscriptionId,
        decimal amount, string currency, DateTime expiresAt)
        : base(id)
    {
        TenantId = tenantId;
        BalanceId = balanceId;
        SubscriptionId = subscriptionId;
        Amount = amount;
        Currency = currency;
        Status = ReservationStatus.Reserved;
        ReservedAt = DateTime.UtcNow;
        ExpiresAt = expiresAt;
        UpdatedAt = DateTime.UtcNow;
    }

    public string TenantId { get; private set; } = string.Empty;
    public Guid BalanceId { get; private set; }
    public Guid SubscriptionId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public ReservationStatus Status { get; private set; }
    public DateTime ReservedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public static Reservation Create(string tenantId, Guid balanceId, Guid subscriptionId,
        decimal amount, string currency, TimeSpan? expiry = null)
    {
        return new Reservation(Guid.NewGuid(), tenantId, balanceId, subscriptionId,
            amount, currency, DateTime.UtcNow + (expiry ?? TimeSpan.FromMinutes(5)));
    }

    public void Debit()
    {
        if (Status != ReservationStatus.Reserved)
            throw new InvalidOperationException($"Cannot debit reservation with status {Status}");
        Status = ReservationStatus.Debited;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Release()
    {
        if (Status != ReservationStatus.Reserved)
            throw new InvalidOperationException($"Cannot release reservation with status {Status}");
        Status = ReservationStatus.Released;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Expire()
    {
        if (Status != ReservationStatus.Reserved)
            return;
        Status = ReservationStatus.Expired;
        UpdatedAt = DateTime.UtcNow;
    }
}
