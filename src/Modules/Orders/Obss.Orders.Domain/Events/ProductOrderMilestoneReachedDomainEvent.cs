using Obss.Orders.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Orders.Domain.Events;

public sealed class ProductOrderMilestoneReachedDomainEvent : DomainEvent
{
    public ProductOrderMilestoneReachedDomainEvent(
        Guid orderId,
        Guid milestoneId,
        string milestoneName,
        MilestoneStatus status)
    {
        OrderId = orderId;
        MilestoneId = milestoneId;
        MilestoneName = milestoneName;
        Status = status;
    }

    public Guid OrderId { get; }
    public Guid MilestoneId { get; }
    public string MilestoneName { get; }
    public MilestoneStatus Status { get; }
}
