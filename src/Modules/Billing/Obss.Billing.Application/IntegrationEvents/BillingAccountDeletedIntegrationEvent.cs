using Obss.SharedKernel.Domain.Events;

namespace Obss.Billing.Application.IntegrationEvents;

public sealed class BillingAccountDeletedIntegrationEvent : IntegrationEvent
{
    public BillingAccountDeletedIntegrationEvent(Guid billingAccountId)
    {
        BillingAccountId = billingAccountId;
    }

    public Guid BillingAccountId { get; }
}
