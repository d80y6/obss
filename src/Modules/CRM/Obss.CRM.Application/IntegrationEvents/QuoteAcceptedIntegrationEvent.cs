using Obss.SharedKernel.Domain.Events;

namespace Obss.CRM.Application.IntegrationEvents;

public sealed class QuoteAcceptedIntegrationEvent : IntegrationEvent
{
    public QuoteAcceptedIntegrationEvent(Guid quoteId, Guid customerId)
    {
        QuoteId = quoteId;
        CustomerId = customerId;
    }

    public Guid QuoteId { get; }
    public Guid CustomerId { get; }
}
