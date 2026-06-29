using Xunit;
using FluentAssertions;
using Obss.Billing.Domain.Entities;

namespace Obss.Billing.Domain.Tests;

public class BillingJobTests
{
    [Fact]
    public void Create_ShouldSetPendingStatus()
    {
        var job = new BillingJob(Guid.NewGuid(), "GenerateBills");

        job.Id.Should().NotBeEmpty();
        job.JobType.Should().Be("GenerateBills");
        job.Status.Should().Be("Pending");
        job.TotalProcessed.Should().Be(0);
        job.TotalErrors.Should().Be(0);
        job.ErrorMessage.Should().BeNull();
        job.StartedAt.Should().BeNull();
        job.CompletedAt.Should().BeNull();
    }

    [Fact]
    public void Start_ShouldSetRunning()
    {
        var job = new BillingJob(Guid.NewGuid(), "GenerateBills");

        job.Start();

        job.Status.Should().Be("Running");
        job.StartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Complete_ShouldSetCompleted()
    {
        var job = new BillingJob(Guid.NewGuid(), "GenerateBills");
        job.Start();

        job.Complete(10, 2);

        job.Status.Should().Be("Completed");
        job.TotalProcessed.Should().Be(10);
        job.TotalErrors.Should().Be(2);
        job.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Fail_ShouldSetFailed()
    {
        var job = new BillingJob(Guid.NewGuid(), "GenerateBills");
        job.Start();

        job.Fail("Database connection timeout");

        job.Status.Should().Be("Failed");
        job.ErrorMessage.Should().Be("Database connection timeout");
        job.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
