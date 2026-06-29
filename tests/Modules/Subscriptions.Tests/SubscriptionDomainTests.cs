using FluentAssertions;
using Obss.Subscriptions.Domain.Entities;
using Xunit;
using Obss.Subscriptions.Domain.Exceptions;
using Obss.Subscriptions.Domain.ValueObjects;

namespace Obss.Subscriptions.Tests;

public class SubscriptionDomainTests
{
    private static Subscription CreatePendingSubscription()
    {
        return Subscription.Create(
            "tenant-1",
            Guid.NewGuid(),
            "John Doe",
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Basic Plan",
            BillingPeriod.Monthly,
            "USD",
            99.99m,
            1,
            DateTime.UtcNow);
    }

    [Fact]
    public void Create_ShouldSetPendingStatus()
    {
        var subscription = CreatePendingSubscription();

        subscription.Status.Should().Be(SubscriptionStatus.Pending);
        subscription.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Activate_ShouldTransitionToActive()
    {
        var subscription = CreatePendingSubscription();

        subscription.Activate();

        subscription.Status.Should().Be(SubscriptionStatus.Active);
        subscription.ActivationDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        subscription.DomainEvents.Should().Contain(e => e.GetType().Name == "SubscriptionActivatedDomainEvent");
    }

    [Fact]
    public void Activate_FromNonPending_ShouldThrow()
    {
        var subscription = CreatePendingSubscription();
        subscription.Activate();

        var act = () => subscription.Activate();

        act.Should().Throw<InvalidSubscriptionStateException>()
            .WithMessage("*Cannot activate*Active*");
    }

    [Fact]
    public void Suspend_FromActive_ShouldTransitionToSuspended()
    {
        var subscription = CreatePendingSubscription();
        subscription.Activate();

        subscription.Suspend("Payment overdue");

        subscription.Status.Should().Be(SubscriptionStatus.Suspended);
        subscription.SuspendedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        subscription.DomainEvents.Should().Contain(e => e.GetType().Name == "SubscriptionSuspendedDomainEvent");
    }

    [Fact]
    public void Suspend_FromNonActive_ShouldThrow()
    {
        var subscription = CreatePendingSubscription();

        var act = () => subscription.Suspend("reason");

        act.Should().Throw<InvalidSubscriptionStateException>()
            .WithMessage("*Cannot suspend*Pending*");
    }

    [Fact]
    public void Resume_FromSuspended_ShouldTransitionToActive()
    {
        var subscription = CreatePendingSubscription();
        subscription.Activate();
        subscription.Suspend("reason");

        subscription.Resume();

        subscription.Status.Should().Be(SubscriptionStatus.Active);
        subscription.SuspendedAt.Should().BeNull();
    }

    [Fact]
    public void Resume_FromNonSuspended_ShouldThrow()
    {
        var subscription = CreatePendingSubscription();
        subscription.Activate();

        var act = () => subscription.Resume();

        act.Should().Throw<InvalidSubscriptionStateException>()
            .WithMessage("*Cannot resume*Active*");
    }

    [Fact]
    public void Cancel_ShouldTransitionToCancelled()
    {
        var subscription = CreatePendingSubscription();
        var effectiveDate = DateTime.UtcNow.AddDays(30);

        subscription.Cancel("Customer request", effectiveDate);

        subscription.Status.Should().Be(SubscriptionStatus.Cancelled);
        subscription.CancelledAt.Should().Be(effectiveDate);
        subscription.EndDate.Should().Be(effectiveDate);
        subscription.DomainEvents.Should().Contain(e => e.GetType().Name == "SubscriptionCancelledDomainEvent");
    }

    [Fact]
    public void Cancel_FromCancelled_ShouldThrow()
    {
        var subscription = CreatePendingSubscription();
        subscription.Cancel("reason", DateTime.UtcNow);

        var act = () => subscription.Cancel("again", DateTime.UtcNow);

        act.Should().Throw<InvalidSubscriptionStateException>();
    }

    [Fact]
    public void Renew_ShouldUpdateRenewalDate()
    {
        var subscription = CreatePendingSubscription();
        subscription.Activate();
        var previousRenewal = subscription.RenewalDate;

        subscription.Renew();

        subscription.RenewalDate.Should().BeAfter(previousRenewal!.Value);
        subscription.DomainEvents.Should().Contain(e => e.GetType().Name == "SubscriptionRenewedDomainEvent");
    }

    [Fact]
    public void Renew_FromNonActive_ShouldThrow()
    {
        var subscription = CreatePendingSubscription();

        var act = () => subscription.Renew();

        act.Should().Throw<InvalidSubscriptionStateException>()
            .WithMessage("*Cannot renew*Pending*");
    }

    [Fact]
    public void ChangeOffer_ShouldUpdateOfferAndPrice()
    {
        var subscription = CreatePendingSubscription();
        subscription.Activate();
        var newOfferId = Guid.NewGuid();

        subscription.ChangeOffer(newOfferId, 149.99m);

        subscription.OfferId.Should().Be(newOfferId);
        subscription.Price.Should().Be(149.99m);
    }

    [Fact]
    public void ChangeQuantity_ShouldValidateMinimum()
    {
        var subscription = CreatePendingSubscription();
        subscription.Activate();

        var act = () => subscription.ChangeQuantity(0);

        act.Should().Throw<ArgumentException>().WithMessage("*at least 1*");
    }

    [Fact]
    public void RecordUsage_ShouldIncreaseUsedAndFireEvents()
    {
        var entitlement = SubscriptionEntitlement.Create(
            Guid.NewGuid(),
            EntitlementType.Bandwidth,
            "Bandwidth",
            1000,
            0,
            "GB",
            false,
            true,
            DateTime.UtcNow);

        entitlement.RecordUsage(250);

        entitlement.Used.Should().Be(250);
        entitlement.DomainEvents.Should().Contain(e => e.GetType().Name == "EntitlementUpdatedDomainEvent");
    }

    [Fact]
    public void RecordUsage_WhenLimitReached_ShouldFireLimitReachedEvent()
    {
        var entitlement = SubscriptionEntitlement.Create(
            Guid.NewGuid(),
            EntitlementType.Bandwidth,
            "Bandwidth",
            100,
            90,
            "GB",
            false,
            true,
            DateTime.UtcNow);

        entitlement.RecordUsage(20);

        entitlement.Used.Should().Be(110);
        entitlement.DomainEvents.Should().Contain(e => e.GetType().Name == "EntitlementLimitReachedDomainEvent");
    }

    [Fact]
    public void IsAvailable_ForUnlimited_ShouldReturnTrue()
    {
        var entitlement = SubscriptionEntitlement.Create(
            Guid.NewGuid(),
            EntitlementType.Storage,
            "Storage",
            0,
            0,
            "GB",
            true,
            false,
            DateTime.UtcNow);

        entitlement.IsAvailable(999999).Should().BeTrue();
    }

    [Fact]
    public void IsAvailable_WithinLimit_ShouldReturnTrue()
    {
        var entitlement = SubscriptionEntitlement.Create(
            Guid.NewGuid(),
            EntitlementType.Users,
            "Users",
            50,
            30,
            "count",
            false,
            false,
            DateTime.UtcNow);

        entitlement.IsAvailable(10).Should().BeTrue();
    }

    [Fact]
    public void IsAvailable_ExceedingLimit_ShouldReturnFalse()
    {
        var entitlement = SubscriptionEntitlement.Create(
            Guid.NewGuid(),
            EntitlementType.Users,
            "Users",
            50,
            45,
            "count",
            false,
            false,
            DateTime.UtcNow);

        entitlement.IsAvailable(10).Should().BeFalse();
    }
}
