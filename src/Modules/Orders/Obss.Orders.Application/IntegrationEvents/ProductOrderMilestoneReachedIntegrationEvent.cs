using Obss.SharedKernel.Domain.Events;

namespace Obss.Orders.Application.IntegrationEvents;

public sealed class ProductOrderMilestoneReachedIntegrationEvent : IntegrationEvent
{
    public ProductOrderMilestoneReachedIntegrationEvent(
        Guid orderId,
        Guid milestoneId,
        string milestoneName,
        string status)
    {
        OrderId = orderId;
        MilestoneId = milestoneId;
        MilestoneName = milestoneName;
        Status = status;
    }

    public Guid OrderId { get; }
    public Guid MilestoneId { get; }
    public string MilestoneName { get; }
    public string Status { get; }
}
