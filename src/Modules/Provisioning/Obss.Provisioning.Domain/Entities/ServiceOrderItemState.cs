namespace Obss.Provisioning.Domain.Entities;

public enum ServiceOrderItemState
{
    Acknowledged,
    InProgress,
    Completed,
    Failed,
    Held,
    Cancelled
}
