using Obss.SharedKernel.Domain.Events;

namespace Obss.Billing.Application.IntegrationEvents;

public sealed class BillingAccountBalanceChangedIntegrationEvent : IntegrationEvent
{
    public BillingAccountBalanceChangedIntegrationEvent(Guid billingAccountId, decimal previousBalance, decimal newBalance, string currency)
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
