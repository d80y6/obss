using Obss.SharedKernel.Domain.Events;

namespace Obss.CRM.Application.IntegrationEvents;

public sealed class QuoteStateChangedIntegrationEvent : IntegrationEvent
{
    public QuoteStateChangedIntegrationEvent(Guid quoteId, string oldState, string newState)
    {
        QuoteId = quoteId;
        OldState = oldState;
        NewState = newState;
    }

    public Guid QuoteId { get; }
    public string OldState { get; }
    public string NewState { get; }
}
