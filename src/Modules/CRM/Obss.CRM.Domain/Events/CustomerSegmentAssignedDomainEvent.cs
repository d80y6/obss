using Obss.SharedKernel.Domain.Common;

namespace Obss.CRM.Domain.Events;

public sealed class CustomerSegmentAssignedDomainEvent : DomainEvent
{
    public CustomerSegmentAssignedDomainEvent(
        Guid segmentId,
        Guid customerId,
        bool isAutoAssigned)
    {
        SegmentId = segmentId;
        CustomerId = customerId;
        IsAutoAssigned = isAutoAssigned;
    }

    public Guid SegmentId { get; }
    public Guid CustomerId { get; }
    public bool IsAutoAssigned { get; }
}
