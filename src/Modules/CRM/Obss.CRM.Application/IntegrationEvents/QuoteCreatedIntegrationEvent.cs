using Obss.SharedKernel.Domain.Events;

namespace Obss.CRM.Application.IntegrationEvents;

public sealed class QuoteCreatedIntegrationEvent : IntegrationEvent
{
    public QuoteCreatedIntegrationEvent(Guid quoteId, Guid customerId)
    {
        QuoteId = quoteId;
        CustomerId = customerId;
    }

    public Guid QuoteId { get; }
    public Guid CustomerId { get; }
}
