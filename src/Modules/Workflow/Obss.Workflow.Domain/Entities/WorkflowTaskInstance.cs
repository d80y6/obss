using Obss.SharedKernel.Domain.Common;
using Obss.Workflow.Domain.ValueObjects;

namespace Obss.Workflow.Domain.Entities;

public class WorkflowTaskInstance : Entity<Guid>
{
    private WorkflowTaskInstance() { }

    public WorkflowTaskInstance(
        Guid id,
        Guid workflowInstanceId,
        Guid workflowStepId,
        string stepName,
        string? assignedTo)
        : base(id)
    {
        WorkflowInstanceId = workflowInstanceId;
        WorkflowStepId = workflowStepId;
        StepName = stepName;
        AssignedTo = assignedTo;
        Status = WorkflowTaskStatus.Pending;
    }

    public Guid WorkflowInstanceId { get; private set; }
    public Guid WorkflowStepId { get; private set; }
    public string StepName { get; private set; } = string.Empty;
    public WorkflowTaskStatus Status { get; private set; }
    public string? AssignedTo { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? Result { get; private set; }
    public string? ErrorMessage { get; private set; }
    public int RetryCount { get; private set; }

    public WorkflowInstance WorkflowInstance { get; private set; } = null!;

    public void Start()
    {
        Status = WorkflowTaskStatus.Running;
        StartedAt = DateTime.UtcNow;
    }

    public void Complete(string? result)
    {
        Status = WorkflowTaskStatus.Completed;
        Result = result;
        CompletedAt = DateTime.UtcNow;
    }

    public void Fail(string error)
    {
        Status = WorkflowTaskStatus.Failed;
        ErrorMessage = error;
        CompletedAt = DateTime.UtcNow;
    }

    public void Skip()
    {
        Status = WorkflowTaskStatus.Skipped;
        CompletedAt = DateTime.UtcNow;
    }

    public void Timeout()
    {
        Status = WorkflowTaskStatus.TimedOut;
        ErrorMessage = "Task timed out.";
        CompletedAt = DateTime.UtcNow;
    }

    public void IncrementRetry()
    {
        RetryCount++;
        Status = WorkflowTaskStatus.Pending;
    }
}
