using Obss.SharedKernel.Domain.Common;

namespace Obss.CRM.Domain.Events;

public sealed class QuoteAcceptedDomainEvent : DomainEvent
{
    public QuoteAcceptedDomainEvent(Guid quoteId, Guid customerId)
    {
        QuoteId = quoteId;
        CustomerId = customerId;
    }

    public Guid QuoteId { get; }
    public Guid CustomerId { get; }
}
