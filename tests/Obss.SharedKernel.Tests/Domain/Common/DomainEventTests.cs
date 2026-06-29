using Xunit;
using FluentAssertions;
using Obss.SharedKernel.Domain.Common;

namespace Obss.SharedKernel.Tests.Domain.Common;

public class DomainEventTests
{
    private sealed class TestDomainEvent : DomainEvent { }

    [Fact]
    public void DomainEvent_EventId_ShouldNotBeEmpty()
    {
        var domainEvent = new TestDomainEvent();

        domainEvent.EventId.Should().NotBeEmpty();
    }

    [Fact]
    public void DomainEvent_OccurredOn_ShouldBeSet()
    {
        var domainEvent = new TestDomainEvent();

        domainEvent.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void TwoDomainEventsWithSameId_ShouldBeEqual()
    {
        var id = Guid.NewGuid();

        var event1 = new TestDomainEvent();
        var event2 = new TestDomainEvent();

        var field = typeof(DomainEvent).GetField("<EventId>k__BackingField",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;

        field.SetValue(event1, id);
        field.SetValue(event2, id);

        event1.Equals(event2).Should().BeTrue();
        (event1 == event2).Should().BeFalse();
    }

    [Fact]
    public void DomainEvent_ShouldNotBeEqual_WhenEventIdsDiffer()
    {
        var event1 = new TestDomainEvent();
        var event2 = new TestDomainEvent();

        event1.EventId.Should().NotBe(event2.EventId);
        event1.Equals(event2).Should().BeFalse();
    }

    [Fact]
    public void DomainEvent_ShouldNotBeEqual_WhenComparedToNull()
    {
        var domainEvent = new TestDomainEvent();

        domainEvent.Equals(null).Should().BeFalse();
    }
}
