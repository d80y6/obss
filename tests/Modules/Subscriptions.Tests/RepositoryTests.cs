using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Obss.Subscriptions.Domain.Entities;
using Obss.Subscriptions.Domain.ValueObjects;
using Obss.Subscriptions.Infrastructure.Persistence.Repositories;

namespace Obss.Subscriptions.Tests;

public class RepositoryTests : SubscriptionIntegrationTests
{
    [Fact]
    public async Task CanAddAndRetrieveSubscription()
    {
        using var context = CreateDbContext();
        var repository = new SubscriptionRepository(context);

        var subscription = Subscription.Create(
            "tenant-1", Guid.NewGuid(), "Jane Doe", Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), Guid.NewGuid(), "Premium", BillingPeriod.Annual,
            "USD", 299.99m, 2, DateTime.UtcNow);

        await repository.AddAsync(subscription);
        await context.SaveChangesAsync();

        var retrieved = await repository.GetByIdAsync(subscription.Id);

        retrieved.Should().NotBeNull();
        retrieved!.CustomerName.Should().Be("Jane Doe");
        retrieved.OfferName.Should().Be("Premium");
        retrieved.Status.Should().Be(SubscriptionStatus.Pending);
        retrieved.BillingPeriod.Should().Be(BillingPeriod.Annual);
        retrieved.Price.Should().Be(299.99m);
        retrieved.Quantity.Should().Be(2);
    }

    [Fact]
    public async Task CanFilterSubscriptionsByStatus()
    {
        using var context = CreateDbContext();
        var repository = new SubscriptionRepository(context);

        var active = Subscription.Create("t1", Guid.NewGuid(), "A", Guid.NewGuid(),
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Plan", BillingPeriod.Monthly,
            "USD", 10, 1, DateTime.UtcNow.AddDays(-10));
        active.Activate();
        active.ClearDomainEvents();

        var pending = Subscription.Create("t1", Guid.NewGuid(), "B", Guid.NewGuid(),
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Plan", BillingPeriod.Monthly,
            "USD", 10, 1, DateTime.UtcNow);

        await context.Subscriptions.AddRangeAsync(active, pending);
        await context.SaveChangesAsync();

        var filtered = await repository.GetFilteredAsync(null, SubscriptionStatus.Active, null, null, 1, 10);

        filtered.Should().HaveCount(1);
        filtered.Should().Contain(s => s.CustomerName == "A");
    }

    [Fact]
    public async Task CanGetActiveByCustomer()
    {
        var customerId = Guid.NewGuid();
        using var context = CreateDbContext();
        var repository = new SubscriptionRepository(context);

        var sub1 = Subscription.Create("t1", customerId, "C1", Guid.NewGuid(),
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Plan", BillingPeriod.Monthly,
            "USD", 10, 1, DateTime.UtcNow.AddDays(-20));
        sub1.Activate();
        sub1.ClearDomainEvents();

        var sub2 = Subscription.Create("t1", customerId, "C2", Guid.NewGuid(),
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Plan", BillingPeriod.Monthly,
            "USD", 10, 1, DateTime.UtcNow);
        sub2.Activate();
        sub2.ClearDomainEvents();

        var otherCustomer = Subscription.Create("t1", Guid.NewGuid(), "Other", Guid.NewGuid(),
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Plan", BillingPeriod.Monthly,
            "USD", 10, 1, DateTime.UtcNow);
        otherCustomer.Activate();
        otherCustomer.ClearDomainEvents();

        await context.Subscriptions.AddRangeAsync(sub1, sub2, otherCustomer);
        await context.SaveChangesAsync();

        var active = await repository.GetActiveByCustomerAsync(customerId);

        active.Should().HaveCount(2);
        active.Should().Contain(s => s.CustomerName == "C1");
        active.Should().Contain(s => s.CustomerName == "C2");
    }

    [Fact]
    public async Task CanAddAndRetrieveEntitlement()
    {
        using var context = CreateDbContext();
        var repository = new SubscriptionEntitlementRepository(context);

        var subscription = Subscription.Create("t1", Guid.NewGuid(), "E Customer", Guid.NewGuid(),
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Entitlement Plan",
            BillingPeriod.Monthly, "USD", 50, 1, DateTime.UtcNow);
        await context.Subscriptions.AddAsync(subscription);
        await context.SaveChangesAsync();

        var entitlement = SubscriptionEntitlement.Create(
            subscription.Id, EntitlementType.Storage, "Storage", 500, 100, "GB", false, true, DateTime.UtcNow);

        await repository.AddAsync(entitlement);
        await context.SaveChangesAsync();

        var retrieved = await repository.GetBySubscriptionAndTypeAsync(subscription.Id, EntitlementType.Storage);

        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Storage");
        retrieved.Limit.Should().Be(500);
        retrieved.Used.Should().Be(100);
    }

    [Fact]
    public async Task CanGetSubscriptionsDueForRenewal()
    {
        using var context = CreateDbContext();
        var repository = new SubscriptionRepository(context);

        var due = Subscription.Create("t1", Guid.NewGuid(), "Due Customer", Guid.NewGuid(),
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Plan", BillingPeriod.Monthly,
            "USD", 10, 1, DateTime.UtcNow.AddMonths(-2));
        due.Activate();
        due.ClearDomainEvents();

        var notDue = Subscription.Create("t1", Guid.NewGuid(), "Not Due", Guid.NewGuid(),
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Plan", BillingPeriod.Monthly,
            "USD", 10, 1, DateTime.UtcNow);
        notDue.Activate();
        notDue.ClearDomainEvents();

        await context.Subscriptions.AddRangeAsync(due, notDue);
        await context.SaveChangesAsync();

        var dueForRenewal = await repository.GetSubscriptionsDueForRenewalAsync(DateTime.UtcNow.AddDays(1));

        dueForRenewal.Should().HaveCount(1);
        dueForRenewal.Should().Contain(s => s.CustomerName == "Due Customer");
    }
}
