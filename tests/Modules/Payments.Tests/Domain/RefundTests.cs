using Xunit;
using FluentAssertions;
using Obss.Payments.Domain.Entities;
using Obss.Payments.Domain.ValueObjects;

namespace Obss.Payments.Tests.Domain;

public class RefundTests
{
    [Fact]
    public void Constructor_ShouldSetPending()
    {
        var refund = new Refund(Guid.NewGuid(), Guid.NewGuid(), 250m, "Customer request", DateTime.UtcNow);

        refund.Status.Should().Be(RefundStatus.Pending);
        refund.Amount.Should().Be(250m);
        refund.CompletedAt.Should().BeNull();
    }

    [Fact]
    public void Complete_ShouldSetCompleted()
    {
        var refund = new Refund(Guid.NewGuid(), Guid.NewGuid(), 250m, "Customer request", DateTime.UtcNow);

        refund.Complete();

        refund.Status.Should().Be(RefundStatus.Completed);
        refund.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void Fail_ShouldSetFailed()
    {
        var refund = new Refund(Guid.NewGuid(), Guid.NewGuid(), 250m, "Customer request", DateTime.UtcNow);

        refund.Fail();

        refund.Status.Should().Be(RefundStatus.Failed);
    }
}
