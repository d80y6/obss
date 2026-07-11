using Xunit;
using FluentAssertions;
using Obss.Orders.Domain.Entities;
using Obss.Orders.Domain.Events;
using Obss.Orders.Domain.ValueObjects;

namespace Obss.Orders.Tests.Domain;

public class ProductOrderMilestoneTests
{
    [Fact]
    public void Constructor_SetsPending()
    {
        var milestone = new ProductOrderMilestone(Guid.NewGuid(), "Custom", "Test", DateTime.UtcNow);
        milestone.Status.Should().Be(MilestoneStatus.Pending);
    }

    [Fact]
    public void Achieve_SetsStatusAndRaisesEvent()
    {
        var milestone = new ProductOrderMilestone(Guid.NewGuid(), "Custom", "Test", DateTime.UtcNow);
        milestone.Achieve();
        milestone.Status.Should().Be(MilestoneStatus.Achieved);
        milestone.DomainEvents.Should().ContainSingle(e => e is ProductOrderMilestoneReachedDomainEvent);
    }

    [Fact]
    public void MarkMissed_SetsMissedStatus()
    {
        var milestone = new ProductOrderMilestone(Guid.NewGuid(), "Custom", "Test", DateTime.UtcNow);
        milestone.MarkMissed();
        milestone.Status.Should().Be(MilestoneStatus.Missed);
    }

    [Fact]
    public void Cancel_SetsCancelledStatus()
    {
        var milestone = new ProductOrderMilestone(Guid.NewGuid(), "Custom", "Test", DateTime.UtcNow);
        milestone.Cancel();
        milestone.Status.Should().Be(MilestoneStatus.Cancelled);
    }
}
