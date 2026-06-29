using Obss.CRM.Domain.Events;
using Obss.CRM.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.CRM.Domain.Entities;

public class CustomerSegment : AggregateRoot<Guid>
{
    private readonly List<CustomerSegmentAssignment> _assignments = [];

    private CustomerSegment() { }

    private CustomerSegment(
        Guid id,
        string tenantId,
        string name,
        string? description,
        SegmentCriteria criteria,
        int priority)
        : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Description = description;
        Criteria = criteria;
        Priority = priority;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public string TenantId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public SegmentCriteria Criteria { get; private set; } = default!;
    public int Priority { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyCollection<CustomerSegmentAssignment> Assignments => _assignments.AsReadOnly();

    public static CustomerSegment Create(
        string tenantId,
        string name,
        string? description,
        SegmentCriteria criteria,
        int priority)
    {
        return new CustomerSegment(
            Guid.NewGuid(),
            tenantId,
            name,
            description,
            criteria,
            priority);
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetCriteria(SegmentCriteria criteria)
    {
        Criteria = criteria;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddCustomer(Guid customerId, Guid assignedBy, bool isAutoAssigned)
    {
        if (_assignments.Any(a => a.CustomerId == customerId))
            return;

        var assignment = new CustomerSegmentAssignment(
            Guid.NewGuid(),
            customerId,
            Id,
            assignedBy,
            isAutoAssigned);

        _assignments.Add(assignment);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CustomerSegmentAssignedDomainEvent(Id, customerId, isAutoAssigned));
    }

    public void RemoveCustomer(Guid customerId)
    {
        var assignment = _assignments.FirstOrDefault(a => a.CustomerId == customerId);
        if (assignment is null)
            return;

        _assignments.Remove(assignment);
        UpdatedAt = DateTime.UtcNow;
    }
}
