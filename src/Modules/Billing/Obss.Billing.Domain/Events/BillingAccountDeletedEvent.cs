using Obss.SharedKernel.Domain.Common;

namespace Obss.Billing.Domain.Events;

public sealed class BillingAccountDeletedEvent : DomainEvent
{
    public BillingAccountDeletedEvent(Guid billingAccountId)
    {
        BillingAccountId = billingAccountId;
    }

    public Guid BillingAccountId { get; }
}
