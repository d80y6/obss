using Obss.SharedKernel.Domain.Common;

namespace Obss.CRM.Domain.Events;

public sealed class QuoteStateChangedDomainEvent : DomainEvent
{
    public QuoteStateChangedDomainEvent(Guid quoteId, string oldState, string newState)
    {
        QuoteId = quoteId;
        OldState = oldState;
        NewState = newState;
    }

    public Guid QuoteId { get; }
    public string OldState { get; }
    public string NewState { get; }
}
