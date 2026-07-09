using Obss.SharedKernel.Domain.Common;

namespace Obss.CRM.Domain.Events;

public sealed class QuoteItemStateChangedDomainEvent : DomainEvent
{
    public QuoteItemStateChangedDomainEvent(Guid quoteId, Guid itemId, string oldState, string newState)
    {
        QuoteId = quoteId;
        ItemId = itemId;
        OldState = oldState;
        NewState = newState;
    }

    public Guid QuoteId { get; }
    public Guid ItemId { get; }
    public string OldState { get; }
    public string NewState { get; }
}
