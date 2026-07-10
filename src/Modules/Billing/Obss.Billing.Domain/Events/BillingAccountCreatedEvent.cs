using Obss.SharedKernel.Domain.Common;

namespace Obss.Billing.Domain.Events;

public sealed class BillingAccountCreatedEvent : DomainEvent
{
    public BillingAccountCreatedEvent(Guid billingAccountId, Guid customerId, string accountType)
    {
        BillingAccountId = billingAccountId;
        CustomerId = customerId;
        AccountType = accountType;
    }

    public Guid BillingAccountId { get; }
    public Guid CustomerId { get; }
    public string AccountType { get; }
}
