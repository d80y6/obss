using Xunit;
using FluentAssertions;
using Obss.Billing.Domain.Entities;
using Obss.Billing.Domain.ValueObjects;

namespace Obss.Billing.Domain.Tests;

public class BillingCycleTests
{
    [Fact]
    public void Create_ShouldSetActiveStatus()
    {
        var customerId = Guid.NewGuid();
        var lastBilling = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var nextBilling = new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc);

        var cycle = BillingCycle.Create(
            "tenant-1",
            customerId,
            BillingPeriod.Monthly,
            lastBilling,
            nextBilling);

        cycle.Id.Should().NotBeEmpty();
        cycle.TenantId.Should().Be("tenant-1");
        cycle.CustomerId.Should().Be(customerId);
        cycle.BillingPeriod.Should().Be(BillingPeriod.Monthly);
        cycle.LastBillingDate.Should().Be(lastBilling);
        cycle.NextBillingDate.Should().Be(nextBilling);
        cycle.Status.Should().Be(BillingCycleStatus.Active);
    }

    [Fact]
    public void AdvanceToNextCycle_Monthly_ShouldAdvanceByOneMonth()
    {
        var cycle = BillingCycle.Create(
            "tenant-1",
            Guid.NewGuid(),
            BillingPeriod.Monthly,
            new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc));

        cycle.AdvanceToNextCycle();

        cycle.LastBillingDate.Should().Be(new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc));
        cycle.NextBillingDate.Should().Be(new DateTime(2025, 3, 1, 0, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void AdvanceToNextCycle_Quarterly_ShouldAdvanceByThreeMonths()
    {
        var cycle = BillingCycle.Create(
            "tenant-1",
            Guid.NewGuid(),
            BillingPeriod.Quarterly,
            new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2025, 4, 1, 0, 0, 0, DateTimeKind.Utc));

        cycle.AdvanceToNextCycle();

        cycle.NextBillingDate.Should().Be(new DateTime(2025, 7, 1, 0, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void AdvanceToNextCycle_SemiAnnual_ShouldAdvanceBySixMonths()
    {
        var cycle = BillingCycle.Create(
            "tenant-1",
            Guid.NewGuid(),
            BillingPeriod.SemiAnnual,
            new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2025, 7, 1, 0, 0, 0, DateTimeKind.Utc));

        cycle.AdvanceToNextCycle();

        cycle.NextBillingDate.Should().Be(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void AdvanceToNextCycle_Annual_ShouldAdvanceByOneYear()
    {
        var cycle = BillingCycle.Create(
            "tenant-1",
            Guid.NewGuid(),
            BillingPeriod.Annual,
            new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        cycle.AdvanceToNextCycle();

        cycle.NextBillingDate.Should().Be(new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void Close_ShouldSetClosedStatus()
    {
        var cycle = BillingCycle.Create(
            "tenant-1",
            Guid.NewGuid(),
            BillingPeriod.Monthly,
            DateTime.UtcNow,
            DateTime.UtcNow.AddMonths(1));

        cycle.Close();

        cycle.Status.Should().Be(BillingCycleStatus.Closed);
    }
}
