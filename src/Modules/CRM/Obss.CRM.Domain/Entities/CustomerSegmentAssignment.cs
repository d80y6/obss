using Obss.SharedKernel.Domain.Common;

namespace Obss.CRM.Domain.Entities;

public class CustomerSegmentAssignment : Entity<Guid>
{
    private CustomerSegmentAssignment() { }

    public CustomerSegmentAssignment(
        Guid id,
        Guid customerId,
        Guid segmentId,
        Guid assignedBy,
        bool isAutoAssigned)
        : base(id)
    {
        CustomerId = customerId;
        SegmentId = segmentId;
        AssignedAt = DateTime.UtcNow;
        AssignedBy = assignedBy;
        IsAutoAssigned = isAutoAssigned;
    }

    public Guid CustomerId { get; private set; }
    public Guid SegmentId { get; private set; }
    public DateTime AssignedAt { get; private set; }
    public Guid AssignedBy { get; private set; }
    public bool IsAutoAssigned { get; private set; }

    public CustomerSegment Segment { get; private set; } = null!;
}
