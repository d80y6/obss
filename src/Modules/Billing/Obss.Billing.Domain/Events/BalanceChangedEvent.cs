using Obss.SharedKernel.Domain.Common;

namespace Obss.Billing.Domain.Events;

public sealed class BalanceChangedEvent : DomainEvent
{
    public BalanceChangedEvent(Guid billingAccountId, decimal previousBalance, decimal newBalance, string currency)
    {
        BillingAccountId = billingAccountId;
        PreviousBalance = previousBalance;
        NewBalance = newBalance;
        Currency = currency;
    }

    public Guid BillingAccountId { get; }
    public decimal PreviousBalance { get; }
    public decimal NewBalance { get; }
    public string Currency { get; }
}
