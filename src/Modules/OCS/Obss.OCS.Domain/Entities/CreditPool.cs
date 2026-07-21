using Obss.OCS.Domain.Exceptions;
using Obss.OCS.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.OCS.Domain.Entities;

public class CreditPool : AggregateRoot<Guid>, ITenantEntity
{
    private CreditPool() { }

    private CreditPool(Guid id, string tenantId, Guid subscriptionId, string name, decimal totalAmount, string currency, DateTime? expiresAt)
        : base(id)
    {
        TenantId = tenantId;
        SubscriptionId = subscriptionId;
        Name = name;
        TotalAmount = totalAmount;
        RemainingAmount = totalAmount;
        Currency = currency;
        Status = CreditPoolStatus.Active;
        CreatedAt = DateTime.UtcNow;
        ExpiresAt = expiresAt;
    }

    public string TenantId { get; private set; } = string.Empty;
    public Guid SubscriptionId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public decimal TotalAmount { get; private set; }
    public decimal RemainingAmount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public CreditPoolStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }

    public static CreditPool Create(string tenantId, Guid subscriptionId, string name, decimal totalAmount, string currency, DateTime? expiresAt = null)
    {
        return new CreditPool(Guid.NewGuid(), tenantId, subscriptionId, name, totalAmount, currency, expiresAt);
    }

    public void Consume(decimal amount)
    {
        if (Status != CreditPoolStatus.Active)
            throw new CreditPoolExhaustedException(Id);
        if (ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value)
        {
            Status = CreditPoolStatus.Expired;
            throw new CreditPoolExhaustedException(Id);
        }
        if (RemainingAmount < amount)
            throw new CreditPoolExhaustedException(Id);
        RemainingAmount -= amount;
        if (RemainingAmount <= 0)
            Status = CreditPoolStatus.Exhausted;
    }

    public void TopUp(decimal amount)
    {
        TotalAmount += amount;
        RemainingAmount += amount;
        Status = CreditPoolStatus.Active;
    }
}
