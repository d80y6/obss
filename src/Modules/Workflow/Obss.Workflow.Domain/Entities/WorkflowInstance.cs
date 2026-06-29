using Obss.SharedKernel.Domain.Common;
using Obss.Workflow.Domain.Events;
using Obss.Workflow.Domain.ValueObjects;

namespace Obss.Workflow.Domain.Entities;

public class WorkflowInstance : AggregateRoot<Guid>
{
    private readonly List<WorkflowTaskInstance> _tasks = [];

    private WorkflowInstance() { }

    private WorkflowInstance(
        Guid id,
        Guid workflowDefinitionId,
        string workflowDefinitionName,
        string triggerEntityType,
        Guid triggerEntityId,
        string createdBy)
        : base(id)
    {
        WorkflowDefinitionId = workflowDefinitionId;
        WorkflowDefinitionName = workflowDefinitionName;
        TriggerEntityType = triggerEntityType;
        TriggerEntityId = triggerEntityId;
        Status = InstanceStatus.Pending;
        CreatedBy = createdBy;
    }

    public Guid WorkflowDefinitionId { get; private set; }
    public string WorkflowDefinitionName { get; private set; } = string.Empty;
    public string TriggerEntityType { get; private set; } = string.Empty;
    public Guid TriggerEntityId { get; private set; }
    public InstanceStatus Status { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime? SlaDeadline { get; private set; }
    public DateTime? SlaBreachedAt { get; private set; }
    public bool IsSlaBreached => SlaBreachedAt.HasValue;

    public IReadOnlyCollection<WorkflowTaskInstance> Tasks => _tasks.AsReadOnly();

    public static WorkflowInstance Create(
        Guid workflowDefinitionId,
        string workflowDefinitionName,
        string triggerEntityType,
        Guid triggerEntityId,
        string createdBy)
    {
        var instance = new WorkflowInstance(
            Guid.NewGuid(),
            workflowDefinitionId,
            workflowDefinitionName,
            triggerEntityType,
            triggerEntityId,
            createdBy);

        instance.AddDomainEvent(new WorkflowStartedDomainEvent(
            instance.Id, workflowDefinitionId, triggerEntityType, triggerEntityId));

        return instance;
    }

    public void Start()
    {
        if (Status != InstanceStatus.Pending)
            throw new InvalidOperationException($"Cannot start workflow in status '{Status}'.");

        Status = InstanceStatus.Running;
        StartedAt = DateTime.UtcNow;
    }

    public void Pause()
    {
        if (Status != InstanceStatus.Running)
            throw new InvalidOperationException($"Cannot pause workflow in status '{Status}'.");

        Status = InstanceStatus.Paused;
    }

    public void Resume()
    {
        if (Status != InstanceStatus.Paused)
            throw new InvalidOperationException($"Cannot resume workflow in status '{Status}'.");

        Status = InstanceStatus.Running;
    }

    public void Complete()
    {
        if (Status != InstanceStatus.Running)
            throw new InvalidOperationException($"Cannot complete workflow in status '{Status}'.");

        Status = InstanceStatus.Completed;
        CompletedAt = DateTime.UtcNow;

        AddDomainEvent(new WorkflowCompletedDomainEvent(Id, WorkflowDefinitionId));
    }

    public void Fail(string error)
    {
        if (Status is InstanceStatus.Completed or InstanceStatus.Cancelled)
            throw new InvalidOperationException($"Cannot fail workflow in status '{Status}'.");

        Status = InstanceStatus.Failed;
        CompletedAt = DateTime.UtcNow;

        AddDomainEvent(new WorkflowFailedDomainEvent(Id, WorkflowDefinitionId, error));
    }

    public void Cancel(string reason)
    {
        if (Status is InstanceStatus.Completed or InstanceStatus.Failed)
            throw new InvalidOperationException($"Cannot cancel workflow in status '{Status}'.");

        Status = InstanceStatus.Cancelled;
        CompletedAt = DateTime.UtcNow;
    }

    public void ApplySlaDefinition(WorkflowSla sla)
    {
        if (Status != InstanceStatus.Running)
            return;

        SlaDeadline = DateTime.UtcNow.AddMinutes(sla.TargetDurationMinutes);
    }

    public void CheckSlaBreach()
    {
        if (!SlaDeadline.HasValue || SlaBreachedAt.HasValue)
            return;

        if (Status is InstanceStatus.Completed or InstanceStatus.Failed or InstanceStatus.Cancelled)
            return;

        if (DateTime.UtcNow > SlaDeadline.Value)
        {
            SlaBreachedAt = DateTime.UtcNow;
            AddDomainEvent(new WorkflowSlaBreachedDomainEvent(Id, WorkflowDefinitionId, SlaDeadline.Value));
        }
    }

    public void AddTask(WorkflowTaskInstance task)
    {
        _tasks.Add(task);
    }

    public void AdvanceToNextStep()
    {
        // Advances workflow to next step - logic depends on step ordering
    }
}
