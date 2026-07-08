using Obss.SharedKernel.Domain.Common;

namespace Obss.Provisioning.Domain.Entities;

public class ServiceOrderItem : Entity<Guid>
{
    internal ServiceOrderItem(
        Guid id,
        Guid serviceOrderId,
        Guid? serviceId,
        ServiceOrderAction action,
        int quantity,
        string? description,
        DateTime? requestedStartDate,
        DateTime? requestedCompletionDate)
        : base(id)
    {
        ServiceOrderId = serviceOrderId;
        ServiceId = serviceId;
        Action = action;
        Quantity = quantity;
        Description = description;
        State = ServiceOrderItemState.Acknowledged;
        RequestedStartDate = requestedStartDate;
        RequestedCompletionDate = requestedCompletionDate;
    }

    private ServiceOrderItem() { }

    public Guid ServiceOrderId { get; private set; }
    public Guid? ServiceId { get; private set; }
    public ServiceOrderAction Action { get; private set; }
    public int Quantity { get; private set; }
    public string? Description { get; private set; }
    public ServiceOrderItemState State { get; private set; }
    public DateTime? RequestedStartDate { get; private set; }
    public DateTime? RequestedCompletionDate { get; private set; }
    public DateTime? CompletedDate { get; private set; }
    public string? ErrorMessage { get; private set; }

    public void Complete(Guid? serviceId = null)
    {
        if (State != ServiceOrderItemState.Acknowledged)
            return;

        State = ServiceOrderItemState.Completed;
        CompletedDate = DateTime.UtcNow;
        if (serviceId.HasValue)
            ServiceId = serviceId;
    }

    public void Fail(string? error = null)
    {
        if (State is ServiceOrderItemState.Completed)
            return;

        State = ServiceOrderItemState.Failed;
        ErrorMessage = error;
        CompletedDate = DateTime.UtcNow;
    }

    public void Hold()
    {
        if (State is ServiceOrderItemState.Completed or ServiceOrderItemState.Failed)
            return;
        State = ServiceOrderItemState.Held;
    }
}
