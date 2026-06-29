using Xunit;
using FluentAssertions;
using Obss.Collections.Domain.Entities;

namespace Obss.Collections.Tests.Domain;

public class DunningPolicyTests
{
    [Fact]
    public void Create_ShouldSetProperties()
    {
        var fees = new Dictionary<int, decimal>
        {
            { 1, 10m },
            { 2, 20m },
            { 3, 30m }
        };

        var policy = DunningPolicy.Create(
            "tenant-1", "Standard Policy", "Standard dunning policy",
            5, fees, 7, 30);

        policy.Id.Should().NotBeEmpty();
        policy.TenantId.Should().Be("tenant-1");
        policy.Name.Should().Be("Standard Policy");
        policy.Description.Should().Be("Standard dunning policy");
        policy.MaxDunningLevel.Should().Be(5);
        policy.DaysBetweenActions.Should().Be(7);
        policy.EscalationAfterDays.Should().Be(30);
        policy.IsActive.Should().BeTrue();
        policy.DunningFees.Should().HaveCount(3);
    }

    [Fact]
    public void Create_WithNullFees_ShouldDefaultToEmpty()
    {
        var policy = DunningPolicy.Create(
            "tenant-1", "No Fees", "", 3, null!, 7, 30);

        policy.DunningFees.Should().BeEmpty();
    }

    [Fact]
    public void Activate_ShouldSetActive()
    {
        var policy = CreateDefaultPolicy();
        policy.Deactivate();

        policy.Activate();

        policy.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_ShouldSetInactive()
    {
        var policy = CreateDefaultPolicy();

        policy.Deactivate();

        policy.IsActive.Should().BeFalse();
    }

    [Fact]
    public void GetFeeForLevel_WithExistingLevel_ShouldReturnFee()
    {
        var fees = new Dictionary<int, decimal> { { 2, 25m } };
        var policy = DunningPolicy.Create(
            "tenant-1", "Test", "", 5, fees, 7, 30);

        policy.GetFeeForLevel(2).Should().Be(25m);
    }

    [Fact]
    public void GetFeeForLevel_WithMissingLevel_ShouldReturnZero()
    {
        var policy = CreateDefaultPolicy();

        policy.GetFeeForLevel(99).Should().Be(0);
    }

    [Fact]
    public void GetNextDunningLevel_WhenBelowMax_ShouldIncrement()
    {
        var fees = new Dictionary<int, decimal> { { 1, 10m } };
        var policy = DunningPolicy.Create(
            "tenant-1", "Test", "", 5, fees, 7, 30);

        policy.GetNextDunningLevel(0).Should().Be(1);
        policy.GetNextDunningLevel(3).Should().Be(4);
    }

    [Fact]
    public void GetNextDunningLevel_WhenAtMax_ShouldCap()
    {
        var fees = new Dictionary<int, decimal> { { 1, 10m } };
        var policy = DunningPolicy.Create(
            "tenant-1", "Test", "", 3, fees, 7, 30);

        policy.GetNextDunningLevel(3).Should().Be(3);
        policy.GetNextDunningLevel(5).Should().Be(5);
    }

    private static DunningPolicy CreateDefaultPolicy()
    {
        return DunningPolicy.Create(
            "tenant-1", "Standard", "Default policy",
            3, new Dictionary<int, decimal> { { 1, 10m } }, 7, 30);
    }
}