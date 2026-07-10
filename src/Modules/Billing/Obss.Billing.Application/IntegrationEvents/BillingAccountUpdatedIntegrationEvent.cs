using Obss.SharedKernel.Domain.Events;

namespace Obss.Billing.Application.IntegrationEvents;

public sealed class BillingAccountUpdatedIntegrationEvent : IntegrationEvent
{
    public BillingAccountUpdatedIntegrationEvent(Guid billingAccountId, string name, string status)
    {
        BillingAccountId = billingAccountId;
        Name = name;
        Status = status;
    }

    public Guid BillingAccountId { get; }
    public string Name { get; }
    public string Status { get; }
}
