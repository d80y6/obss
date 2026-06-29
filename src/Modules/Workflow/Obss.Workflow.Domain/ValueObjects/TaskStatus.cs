namespace Obss.Workflow.Domain.ValueObjects;

public enum WorkflowTaskStatus
{
    Pending,
    Running,
    Completed,
    Failed,
    Skipped,
    TimedOut
}
