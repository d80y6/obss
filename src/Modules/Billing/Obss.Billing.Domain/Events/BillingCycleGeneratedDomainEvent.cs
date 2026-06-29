using Obss.Billing.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Billing.Domain.Events;

public sealed class BillingCycleGeneratedDomainEvent : DomainEvent
{
    public BillingCycleGeneratedDomainEvent(Guid customerId, BillingPeriod billingPeriod)
    {
        CustomerId = customerId;
        BillingPeriod = billingPeriod;
    }

    public Guid CustomerId { get; }
    public BillingPeriod BillingPeriod { get; }
}
