using Obss.Orders.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Orders.Domain.Entities;

public class OrderFulfillment : AggregateRoot<Guid>
{
    private OrderFulfillment() { }

    private OrderFulfillment(
        Guid id,
        Guid orderId)
        : base(id)
    {
        OrderId = orderId;
        Status = FulfillmentStatus.Pending;
        StartedAt = DateTime.UtcNow;
    }

    public Guid OrderId { get; private set; }
    public FulfillmentStatus Status { get; private set; }
    public Guid? WorkflowInstanceId { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? ErrorMessage { get; private set; }

    public static OrderFulfillment Create(Guid orderId)
    {
        return new OrderFulfillment(Guid.NewGuid(), orderId);
    }

    public void StartFulfillment(Guid workflowInstanceId)
    {
        if (Status != FulfillmentStatus.Pending)
            throw new InvalidOperationException($"Cannot start fulfillment in {Status} status.");

        Status = FulfillmentStatus.InProgress;
        WorkflowInstanceId = workflowInstanceId;
    }

    public void Complete()
    {
        if (Status == FulfillmentStatus.Completed)
            throw new InvalidOperationException("Fulfillment is already completed.");

        if (Status == FulfillmentStatus.Pending)
            Status = FulfillmentStatus.InProgress;

        Status = FulfillmentStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    public void Fail(string error)
    {
        if (Status == FulfillmentStatus.Completed)
            throw new InvalidOperationException("Cannot fail a completed fulfillment.");

        Status = FulfillmentStatus.Failed;
        CompletedAt = DateTime.UtcNow;
        ErrorMessage = error;
    }
}
