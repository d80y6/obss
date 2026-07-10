using Obss.Billing.Domain.Events;
using Obss.Billing.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Billing.Domain.Entities;

public class AccountBalance : AggregateRoot<Guid>
{
    private readonly List<BalanceTransaction> _transactions = [];

    private AccountBalance() { }

    public AccountBalance(Guid billingAccountId, decimal currentBalance, decimal outstandingBalance, decimal availableCredit, string currency)
    {
        Id = Guid.NewGuid();
        BillingAccountId = billingAccountId;
        CurrentBalance = currentBalance;
        OutstandingBalance = outstandingBalance;
        AvailableCredit = availableCredit;
        Currency = currency;
        BalanceDate = DateTime.UtcNow;
        LastUpdatedAt = DateTime.UtcNow;
    }

    public Guid BillingAccountId { get; private set; }
    public decimal CurrentBalance { get; private set; }
    public decimal OutstandingBalance { get; private set; }
    public decimal AvailableCredit { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public DateTime BalanceDate { get; private set; }
    public DateTime LastUpdatedAt { get; private set; }
    public string? AtType { get; private set; } = "AccountBalance";
    public string? AtBaseType { get; private set; } = "PartyBalance";
#pragma warning disable S1144 // Used by EF Core via reflection
    public string? AtSchemaLocation { get; private set; }
#pragma warning restore S1144

    public IReadOnlyCollection<BalanceTransaction> Transactions => _transactions.AsReadOnly();

    public void RecordTransaction(decimal amount, TransactionType type, string description, string? referenceId, string? referenceType)
    {
        var previousBalance = CurrentBalance;
        var signedAmount = type switch
        {
            TransactionType.Payment or TransactionType.Credit or TransactionType.Refund => -Math.Abs(amount),
            TransactionType.Charge or TransactionType.Debit => Math.Abs(amount),
            TransactionType.Adjustment => amount,
            _ => amount
        };
        CurrentBalance += signedAmount;
        LastUpdatedAt = DateTime.UtcNow;
        _transactions.Add(new BalanceTransaction(Id, signedAmount, type, description, referenceId, referenceType));
        AddDomainEvent(new BalanceChangedEvent(BillingAccountId, previousBalance, CurrentBalance, Currency));
    }
}
