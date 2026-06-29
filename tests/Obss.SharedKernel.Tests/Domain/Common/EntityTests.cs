using Xunit;
using FluentAssertions;
using Obss.SharedKernel.Domain.Common;

namespace Obss.SharedKernel.Tests.Domain.Common;

public class EntityTests
{
    private sealed class TestEntity : Entity<Guid>
    {
        public TestEntity(Guid id) : base(id) { }
        public TestEntity() { }

        public void AddTestDomainEvent(DomainEvent domainEvent) => AddDomainEvent(domainEvent);
    }

    private sealed class TestDomainEvent : DomainEvent { }

    [Fact]
    public void TwoEntitiesWithSameId_ShouldBeEqual()
    {
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id);
        var entity2 = new TestEntity(id);

        (entity1 == entity2).Should().BeTrue();
        entity1.Equals(entity2).Should().BeTrue();
        entity1.Equals((object)entity2).Should().BeTrue();
        entity1.GetHashCode().Should().Be(entity2.GetHashCode());
    }

    [Fact]
    public void TwoEntitiesWithDifferentIds_ShouldNotBeEqual()
    {
        var entity1 = new TestEntity(Guid.NewGuid());
        var entity2 = new TestEntity(Guid.NewGuid());

        (entity1 != entity2).Should().BeTrue();
        entity1.Equals(entity2).Should().BeFalse();
    }

    [Fact]
    public void Entity_ShouldNotBeEqual_WhenComparedToNull()
    {
        var entity = new TestEntity(Guid.NewGuid());

        entity.Equals(null as TestEntity).Should().BeFalse();
        (null as TestEntity == entity).Should().BeFalse();
    }

    [Fact]
    public void NullEntities_ShouldBeEqual_WhenBothNull()
    {
        TestEntity? entity1 = null;
        TestEntity? entity2 = null;

        (entity1 == entity2).Should().BeTrue();
    }

    [Fact]
    public void DomainEvents_CanBeAdded()
    {
        var entity = new TestEntity(Guid.NewGuid());
        var domainEvent = new TestDomainEvent();

        entity.AddTestDomainEvent(domainEvent);

        entity.DomainEvents.Should().ContainSingle().Which.Should().Be(domainEvent);
    }

    [Fact]
    public void DomainEvents_CanBeCleared()
    {
        var entity = new TestEntity(Guid.NewGuid());
        entity.AddTestDomainEvent(new TestDomainEvent());
        entity.AddTestDomainEvent(new TestDomainEvent());

        entity.ClearDomainEvents();

        entity.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void DomainEvents_ReturnsReadOnlyCollection()
    {
        var entity = new TestEntity(Guid.NewGuid());
        entity.AddTestDomainEvent(new TestDomainEvent());

        var events = entity.DomainEvents;
        events.Should().BeAssignableTo<IReadOnlyCollection<DomainEvent>>();
    }

    [Fact]
    public void MultipleDomainEvents_ShouldAllBeStored()
    {
        var entity = new TestEntity(Guid.NewGuid());
        var event1 = new TestDomainEvent();
        var event2 = new TestDomainEvent();

        entity.AddTestDomainEvent(event1);
        entity.AddTestDomainEvent(event2);

        entity.DomainEvents.Should().HaveCount(2);
        entity.DomainEvents.Should().ContainInOrder(event1, event2);
    }
}
