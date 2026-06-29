using Xunit;
using FluentAssertions;
using Obss.Collections.Domain.Entities;
using Obss.Collections.Domain.ValueObjects;

namespace Obss.Collections.Tests.Domain;

public class CollectionActionTests
{
    [Fact]
    public void Create_ShouldSetProperties()
    {
        var caseId = Guid.NewGuid();
        var nextAction = DateTime.UtcNow.AddDays(7);

        var action = CollectionAction.Create(
            caseId, CollectionActionType.PhoneCall, 1,
            "Called customer regarding overdue payment", "agent-1", nextAction);

        action.Id.Should().NotBeEmpty();
        action.CollectionCaseId.Should().Be(caseId);
        action.ActionType.Should().Be(CollectionActionType.PhoneCall);
        action.DunningLevel.Should().Be(1);
        action.Description.Should().Be("Called customer regarding overdue payment");
        action.PerformedBy.Should().Be("agent-1");
        action.PerformedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        action.NextActionDate.Should().Be(nextAction);
        action.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public void Create_WithoutNextActionDate_ShouldDefaultToNull()
    {
        var action = CollectionAction.Create(
            Guid.NewGuid(), CollectionActionType.Email, 0, "Sent reminder", "system");

        action.NextActionDate.Should().BeNull();
    }

    [Fact]
    public void Complete_ShouldSetIsCompleted()
    {
        var action = CollectionAction.Create(
            Guid.NewGuid(), CollectionActionType.Visit, 1, "Visited customer", "agent-1");
        action.Complete();

        action.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public void Reschedule_ShouldSetNewDateAndSetIncomplete()
    {
        var action = CollectionAction.Create(
            Guid.NewGuid(), CollectionActionType.PhoneCall, 0, "Call back", "agent-1");
        var newDate = DateTime.UtcNow.AddDays(3);

        action.Reschedule(newDate);

        action.NextActionDate.Should().Be(newDate);
        action.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public void Create_WithAllActionTypes_ShouldSucceed()
    {
        var caseId = Guid.NewGuid();
        foreach (CollectionActionType type in Enum.GetValues<CollectionActionType>())
        {
            var action = CollectionAction.Create(caseId, type, 0, $"Test {type}", "system");
            action.ActionType.Should().Be(type);
        }
    }
}