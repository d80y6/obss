using Obss.SharedKernel.Domain.Common;

namespace Obss.Billing.Domain.Events;

public sealed class BillingAccountUpdatedEvent : DomainEvent
{
    public BillingAccountUpdatedEvent(Guid billingAccountId, string name, string status)
    {
        BillingAccountId = billingAccountId;
        Name = name;
        Status = status;
    }

    public Guid BillingAccountId { get; }
    public string Name { get; }
    public string Status { get; }
}
