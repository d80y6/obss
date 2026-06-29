using Xunit;
using FluentAssertions;
using Obss.Rating.Domain.Entities;
using Obss.Rating.Domain.ValueObjects;

namespace Obss.Rating.Tests.Domain;

public class UsageRecordTests
{
    [Fact]
    public void Create_ShouldSetPropertiesCorrectly()
    {
        var subscriptionId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var startTime = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var endTime = new DateTime(2025, 1, 1, 10, 5, 0, DateTimeKind.Utc);

        var record = UsageRecord.Create(
            "tenant-1", subscriptionId, serviceId, RecordType.Voice, "local-call",
            startTime, endTime, 300, 0, "1234567890", "0987654321", "USD");

        record.Id.Should().NotBeEmpty();
        record.TenantId.Should().Be("tenant-1");
        record.SubscriptionId.Should().Be(subscriptionId);
        record.ServiceId.Should().Be(serviceId);
        record.RecordType.Should().Be(RecordType.Voice);
        record.UsageType.Should().Be("local-call");
        record.StartTime.Should().Be(startTime);
        record.EndTime.Should().Be(endTime);
        record.Duration.Should().Be(300);
        record.Status.Should().Be(UsageStatus.Unrated);
        record.RatedAmount.Should().Be(0);
    }

    [Fact]
    public void MarkAsRated_ShouldUpdateStatusAndAmount()
    {
        var record = CreateDefaultRecord();
        var ruleId = Guid.NewGuid();

        record.MarkAsRated(50.0m, ruleId);

        record.Status.Should().Be(UsageStatus.Rated);
        record.RatedAmount.Should().Be(50.0m);
        record.RatingRuleId.Should().Be(ruleId);
        record.RatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void MarkAsError_ShouldSetErrorMessage()
    {
        var record = CreateDefaultRecord();

        record.MarkAsError("Invalid rating rule");

        record.Status.Should().Be(UsageStatus.Error);
        record.ErrorMessage.Should().Be("Invalid rating rule");
        record.RatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void MarkAsRated_WhenAlreadyRated_ShouldOverwrite()
    {
        var record = CreateDefaultRecord();
        record.MarkAsRated(50.0m, Guid.NewGuid());

        var newRuleId = Guid.NewGuid();
        record.MarkAsRated(75.0m, newRuleId);

        record.RatedAmount.Should().Be(75.0m);
        record.RatingRuleId.Should().Be(newRuleId);
    }

    private static UsageRecord CreateDefaultRecord()
    {
        return UsageRecord.Create(
            "tenant-1", Guid.NewGuid(), Guid.NewGuid(), RecordType.Data, "internet",
            DateTime.UtcNow.AddHours(-1), DateTime.UtcNow, 3600, 1073741824,
            "src-host", "dst-host", "USD");
    }
}
