using Xunit;
using FluentAssertions;
using Obss.Collections.Domain.Entities;
using Obss.Collections.Domain.ValueObjects;

namespace Obss.Collections.Tests.Domain;

public class InstallmentTests
{
    [Fact]
    public void Create_ShouldSetProperties()
    {
        var arrangementId = Guid.NewGuid();
        var dueDate = new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc);

        var installment = Installment.Create(arrangementId, 1, dueDate, 250m);

        installment.Id.Should().NotBeEmpty();
        installment.PaymentArrangementId.Should().Be(arrangementId);
        installment.InstallmentNumber.Should().Be(1);
        installment.DueDate.Should().Be(dueDate);
        installment.Amount.Should().Be(250m);
        installment.PaidAmount.Should().Be(0);
        installment.Status.Should().Be(InstallmentStatus.Pending);
        installment.PaidAt.Should().BeNull();
    }

    [Fact]
    public void RecordPayment_ShouldUpdateStatusAndAmount()
    {
        var installment = Installment.Create(Guid.NewGuid(), 1, DateTime.UtcNow, 100m);

        installment.RecordPayment(100m);

        installment.PaidAmount.Should().Be(100m);
        installment.Status.Should().Be(InstallmentStatus.Paid);
        installment.PaidAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void MarkOverdue_WhenPending_ShouldTransition()
    {
        var installment = Installment.Create(Guid.NewGuid(), 1, DateTime.UtcNow, 100m);

        installment.MarkOverdue();

        installment.Status.Should().Be(InstallmentStatus.Overdue);
    }

    [Fact]
    public void MarkOverdue_WhenAlreadyPaid_ShouldNotChange()
    {
        var installment = Installment.Create(Guid.NewGuid(), 1, DateTime.UtcNow, 100m);
        installment.RecordPayment(100m);

        installment.MarkOverdue();

        installment.Status.Should().Be(InstallmentStatus.Paid);
    }

    [Fact]
    public void MarkDefaulted_ShouldSetStatus()
    {
        var installment = Installment.Create(Guid.NewGuid(), 1, DateTime.UtcNow, 100m);

        installment.MarkDefaulted();

        installment.Status.Should().Be(InstallmentStatus.Defaulted);
    }

    [Fact]
    public void MarkDefaulted_FromAnyStatus_ShouldOverride()
    {
        var installment = Installment.Create(Guid.NewGuid(), 1, DateTime.UtcNow, 100m);
        installment.RecordPayment(100m);

        installment.MarkDefaulted();

        installment.Status.Should().Be(InstallmentStatus.Defaulted);
    }
}
