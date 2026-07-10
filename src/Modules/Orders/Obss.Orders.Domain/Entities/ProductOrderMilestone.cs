using Obss.Orders.Domain.Events;
using Obss.Orders.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Orders.Domain.Entities;

public class ProductOrderMilestone : Entity<Guid>
{
    public Guid ProductOrderId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DateTime MilestoneDate { get; private set; }
    public MilestoneStatus Status { get; private set; }

    private ProductOrderMilestone() { }

    public ProductOrderMilestone(Guid productOrderId, string name, string description, DateTime milestoneDate)
        : base(Guid.NewGuid())
    {
        ProductOrderId = productOrderId;
        Name = name;
        Description = description;
        MilestoneDate = milestoneDate;
        Status = MilestoneStatus.Pending;
    }

    public void Achieve()
    {
        Status = MilestoneStatus.Achieved;
        AddDomainEvent(new ProductOrderMilestoneReachedDomainEvent(ProductOrderId, Id, Name, MilestoneStatus.Achieved));
    }

    public void MarkMissed()
    {
        Status = MilestoneStatus.Missed;
        AddDomainEvent(new ProductOrderMilestoneReachedDomainEvent(ProductOrderId, Id, Name, MilestoneStatus.Missed));
    }

    public void Cancel()
    {
        Status = MilestoneStatus.Cancelled;
    }
}
