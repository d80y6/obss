namespace Obss.Provisioning.Domain.ValueObjects;

public enum JobStatus
{
    Pending,
    Queued,
    InProgress,
    Completed,
    Failed,
    RolledBack
}
