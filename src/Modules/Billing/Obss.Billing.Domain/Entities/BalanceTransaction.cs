using Obss.Billing.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Billing.Domain.Entities;

public class BalanceTransaction : Entity<Guid>
{
    private BalanceTransaction() { }

    public BalanceTransaction(Guid balanceId, decimal amount, TransactionType transactionType, string description, string? referenceId, string? referenceType)
    {
        Id = Guid.NewGuid();
        BalanceId = balanceId;
        Amount = amount;
        TransactionType = transactionType;
        Description = description;
        TransactionDate = DateTime.UtcNow;
        ReferenceId = referenceId;
        ReferenceType = referenceType;
    }

    public Guid BalanceId { get; private set; }
    public decimal Amount { get; private set; }
    public TransactionType TransactionType { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public DateTime TransactionDate { get; private set; }
    public string? ReferenceId { get; private set; }
    public string? ReferenceType { get; private set; }
}
