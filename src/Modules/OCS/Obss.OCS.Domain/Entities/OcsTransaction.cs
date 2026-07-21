using Obss.OCS.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.OCS.Domain.Entities;

public class OcsTransaction : AggregateRoot<Guid>, ITenantEntity
{
    private OcsTransaction() { }

    private OcsTransaction(Guid id, string tenantId, Guid subscriptionId, Guid? balanceId, Guid? creditPoolId,
        TransactionType transactionType, decimal amount, string currency, string description)
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
    public DateTime Timestamp { get; private set; }

    public static OcsTransaction Create(string tenantId, Guid subscriptionId, Guid? balanceId, Guid? creditPoolId,
        TransactionType transactionType, decimal amount, string currency, string description)
    {
        return new OcsTransaction(Guid.NewGuid(), tenantId, subscriptionId, balanceId, creditPoolId,
            transactionType, amount, currency, description);
    }
}
