namespace Obss.Workflow.Domain.ValueObjects;

public enum InstanceStatus
{
    Pending,
    Running,
    Paused,
    Completed,
    Failed,
    Cancelled
}
