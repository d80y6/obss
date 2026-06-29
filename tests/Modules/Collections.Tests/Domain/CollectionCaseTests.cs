using Xunit;
using FluentAssertions;
using Obss.Collections.Domain.Entities;
using Obss.Collections.Domain.Events;
using Obss.Collections.Domain.Exceptions;
using Obss.Collections.Domain.ValueObjects;

namespace Obss.Collections.Tests.Domain;

public class CollectionCaseTests
{
    [Fact]
    public void Open_ShouldSetPropertiesCorrectly()
    {
        var customerId = Guid.NewGuid();
        var @case = CollectionCase.Open("tenant-1", customerId, "Test Customer", 1500m, "USD");

        @case.Id.Should().NotBeEmpty();
        @case.TenantId.Should().Be("tenant-1");
        @case.CustomerId.Should().Be(customerId);
        @case.CustomerName.Should().Be("Test Customer");
        @case.TotalOverdueAmount.Should().Be(1500m);
        @case.Currency.Should().Be("USD");
        @case.Status.Should().Be(CollectionCaseStatus.Open);
        @case.CurrentDunningLevel.Should().Be(0);
        @case.Actions.Should().BeEmpty();
        @case.PaymentArrangements.Should().BeEmpty();
    }

    [Fact]
    public void Open_ShouldRaiseCollectionCaseOpenedDomainEvent()
    {
        var customerId = Guid.NewGuid();
        var @case = CollectionCase.Open("tenant-1", customerId, "Test Customer", 1500m, "USD");

        var events = @case.DomainEvents;
        events.Should().ContainSingle(e => e is CollectionCaseOpenedDomainEvent);
        var domainEvent = events.OfType<CollectionCaseOpenedDomainEvent>().Single();
        domainEvent.CaseId.Should().Be(@case.Id);
        domainEvent.CustomerId.Should().Be(customerId);
        domainEvent.CustomerName.Should().Be("Test Customer");
        domainEvent.TotalOverdueAmount.Should().Be(1500m);
        domainEvent.Currency.Should().Be("USD");
    }

    [Fact]
    public void AssignTo_ShouldSetAssignee()
    {
        var @case = CreateOpenCase();
        @case.AssignTo("user-1");

        @case.AssignedTo.Should().Be("user-1");
        @case.LastActionAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void AssignTo_WhenResolved_ShouldThrow()
    {
        var @case = CreateOpenCase();
        @case.Resolve();

        var act = () => @case.AssignTo("user-1");
        act.Should().Throw<InvalidCollectionStateException>().WithMessage("*Cannot assign a resolved or closed case*");
    }

    [Fact]
    public void AssignTo_WhenClosed_ShouldThrow()
    {
        var @case = CreateOpenCase();
        @case.Resolve();
        @case.Close();

        var act = () => @case.AssignTo("user-1");
        act.Should().Throw<InvalidCollectionStateException>().WithMessage("*Cannot assign a resolved or closed case*");
    }

    [Fact]
    public void AddAction_ShouldTransitionToInProgress()
    {
        var @case = CreateOpenCase();
        var action = CreateAction(@case.Id);

        @case.AddAction(action);

        @case.Status.Should().Be(CollectionCaseStatus.InProgress);
        @case.Actions.Should().ContainSingle();
        @case.LastActionAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void AddAction_WithEscalation_ShouldAdvanceDunningLevel()
    {
        var @case = CreateOpenCase();
        var escalation = CollectionAction.Create(
            @case.Id, CollectionActionType.Escalation, 1,
            "Escalation", "system");

        @case.AddAction(escalation);

        @case.CurrentDunningLevel.Should().Be(1);
    }

    [Fact]
    public void AddAction_WithoutEscalation_ShouldNotAdvanceDunningLevel()
    {
        var @case = CreateOpenCase();
        var action = CreateAction(@case.Id);

        @case.AddAction(action);

        @case.CurrentDunningLevel.Should().Be(0);
    }

    [Fact]
    public void CreatePaymentArrangement_ShouldAddArrangementAndRaiseEvent()
    {
        var @case = CreateOpenCase();
        var firstPayment = DateTime.UtcNow.AddDays(7);

        var arrangement = @case.CreatePaymentArrangement(1000m, 4, 250m, PaymentFrequency.Weekly, firstPayment);

        arrangement.Should().NotBeNull();
        arrangement.TotalAmount.Should().Be(1000m);
        arrangement.InstallmentCount.Should().Be(4);
        @case.PaymentArrangements.Should().ContainSingle();
        @case.DomainEvents.Should().Contain(e => e is PaymentArrangementCreatedDomainEvent);
    }

    [Fact]
    public void CreatePaymentArrangement_WhenClosed_ShouldThrow()
    {
        var @case = CreateOpenCase();
        @case.Resolve();
        @case.Close();

        var act = () => @case.CreatePaymentArrangement(1000m, 4, 250m, PaymentFrequency.Weekly, DateTime.UtcNow.AddDays(7));
        act.Should().Throw<InvalidCollectionStateException>();
    }

    [Fact]
    public void CreatePaymentArrangement_WithDuplicateActive_ShouldThrow()
    {
        var @case = CreateOpenCase();
        @case.CreatePaymentArrangement(1000m, 4, 250m, PaymentFrequency.Weekly, DateTime.UtcNow.AddDays(7));

        var act = () => @case.CreatePaymentArrangement(500m, 2, 250m, PaymentFrequency.Weekly, DateTime.UtcNow.AddDays(7));
        act.Should().Throw<DuplicatePaymentArrangementException>();
    }

    [Fact]
    public void UpdateOverdueAmount_ShouldUpdate()
    {
        var @case = CreateOpenCase();
        @case.UpdateOverdueAmount(2000m);

        @case.TotalOverdueAmount.Should().Be(2000m);
    }

    [Fact]
    public void Resolve_ShouldSetResolvedStatusAndRaiseEvent()
    {
        var @case = CreateOpenCase();
        @case.AddAction(CreateAction(@case.Id));

        @case.Resolve();

        @case.Status.Should().Be(CollectionCaseStatus.Resolved);
        @case.ResolvedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        @case.DomainEvents.Should().Contain(e => e is CollectionCaseResolvedDomainEvent);
    }

    [Fact]
    public void Resolve_WhenAlreadyResolved_ShouldThrow()
    {
        var @case = CreateOpenCase();
        @case.AddAction(CreateAction(@case.Id));
        @case.Resolve();

        var act = () => @case.Resolve();
        act.Should().Throw<InvalidCollectionStateException>().WithMessage("*Case is already resolved or closed*");
    }

    [Fact]
    public void Close_WhenResolved_ShouldSucceed()
    {
        var @case = CreateOpenCase();
        @case.AddAction(CreateAction(@case.Id));
        @case.Resolve();

        @case.Close();

        @case.Status.Should().Be(CollectionCaseStatus.Closed);
    }

    [Fact]
    public void Close_WhenNotResolved_ShouldThrow()
    {
        var @case = CreateOpenCase();

        var act = () => @case.Close();
        act.Should().Throw<InvalidCollectionStateException>().WithMessage("*Only resolved cases can be closed*");
    }

    [Fact]
    public void AddNote_ShouldAppend()
    {
        var @case = CreateOpenCase();

        @case.AddNote("First note");
        @case.AddNote("Second note");

        @case.Notes.Should().Be("First note\nSecond note");
    }

    [Fact]
    public void AdvanceDunningLevel_ShouldIncrement()
    {
        var @case = CreateOpenCase();

        @case.AdvanceDunningLevel();
        @case.AdvanceDunningLevel();

        @case.CurrentDunningLevel.Should().Be(2);
    }

    private static CollectionCase CreateOpenCase()
    {
        return CollectionCase.Open("tenant-1", Guid.NewGuid(), "Test Customer", 1500m, "USD");
    }

    private static CollectionAction CreateAction(Guid caseId)
    {
        return CollectionAction.Create(
            caseId, CollectionActionType.PhoneCall, 0, "Called customer", "agent-1");
    }
}