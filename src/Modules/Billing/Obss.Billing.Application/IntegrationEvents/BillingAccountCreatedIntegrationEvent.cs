using Obss.SharedKernel.Domain.Events;

namespace Obss.Billing.Application.IntegrationEvents;

public sealed class BillingAccountCreatedIntegrationEvent : IntegrationEvent
{
    public BillingAccountCreatedIntegrationEvent(Guid billingAccountId, Guid customerId, string accountType)
    {
        BillingAccountId = billingAccountId;
        CustomerId = customerId;
        AccountType = accountType;
    }

    public Guid BillingAccountId { get; }
    public Guid CustomerId { get; }
    public string AccountType { get; }
}
