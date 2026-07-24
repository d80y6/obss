using Obss.OCS.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.OCS.Domain.Entities;

public class OcsTransaction : AggregateRoot<Guid>, ITenantEntity
{
    private OcsTransaction() { }

    private OcsTransaction(Guid id, string tenantId, Guid subscriptionId, Guid? balanceId, Guid? creditPoolId,
        TransactionType transactionType, decimal amount, string currency, string description,
        string? correlationId = null, Guid? reservationId = null, decimal beforeBalance = 0, decimal afterBalance = 0)
        : base(id)
    {
        TenantId = tenantId;
        SubscriptionId = subscriptionId;
        BalanceId = balanceId;
        CreditPoolId = creditPoolId;
        TransactionType = transactionType;
        Amount = amount;
        Currency = currency;
        Description = description;
        CorrelationId = correlationId;
        ReservationId = reservationId;
        BeforeBalance = beforeBalance;
        AfterBalance = afterBalance;
        Timestamp = DateTime.UtcNow;
    }

    public string TenantId { get; private set; } = string.Empty;
    public Guid SubscriptionId { get; private set; }
    public Guid? BalanceId { get; private set; }
    public Guid? CreditPoolId { get; private set; }
    public TransactionType TransactionType { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string? CorrelationId { get; private set; }
    public Guid? ReservationId { get; private set; }
    public decimal BeforeBalance { get; private set; }
    public decimal AfterBalance { get; private set; }
    public DateTime Timestamp { get; private set; }

    public static OcsTransaction Create(string tenantId, Guid subscriptionId, Guid? balanceId, Guid? creditPoolId,
        TransactionType transactionType, decimal amount, string currency, string description,
        string? correlationId = null, Guid? reservationId = null, decimal beforeBalance = 0, decimal afterBalance = 0)
    {
        return new OcsTransaction(Guid.NewGuid(), tenantId, subscriptionId, balanceId, creditPoolId,
            transactionType, amount, currency, description, correlationId, reservationId, beforeBalance, afterBalance);
    }
}
