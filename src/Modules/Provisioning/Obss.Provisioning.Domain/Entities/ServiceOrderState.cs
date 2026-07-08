namespace Obss.Provisioning.Domain.Entities;

public enum ServiceOrderState
{
    Acknowledged,
    InProgress,
    Held,
    Completed,
    Failed,
    PartiallyCompleted,
    Cancelled,
    Rejected,
    PendingCancellation
}
