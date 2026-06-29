using Xunit;
using FluentAssertions;
using Obss.Collections.Domain.Entities;
using Obss.Collections.Domain.Events;
using Obss.Collections.Domain.Exceptions;
using Obss.Collections.Domain.ValueObjects;

namespace Obss.Collections.Tests.Domain;

public class PaymentArrangementTests
{
    [Fact]
    public void Create_ShouldGenerateInstallments()
    {
        var arrangement = CreateArrangement(PaymentFrequency.Monthly);

        arrangement.Status.Should().Be(ArrangementStatus.Proposed);
        arrangement.Installments.Should().HaveCount(6);
        arrangement.Installments.Should().AllSatisfy(i =>
        {
            i.PaymentArrangementId.Should().Be(arrangement.Id);
            i.Amount.Should().Be(100m);
            i.Status.Should().Be(InstallmentStatus.Pending);
        });
    }

    [Fact]
    public void Create_WithWeeklyFrequency_ShouldSpaceInstallmentsWeekly()
    {
        var firstPayment = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var arrangement = PaymentArrangement.Create(
            Guid.NewGuid(), Guid.NewGuid(), 200m, 4, 50m,
            PaymentFrequency.Weekly, firstPayment);

        var installments = arrangement.Installments.ToList();
        installments.Should().HaveCount(4);
        installments[0].DueDate.Should().Be(firstPayment);
        installments[1].DueDate.Should().Be(firstPayment.AddDays(7));
        installments[2].DueDate.Should().Be(firstPayment.AddDays(14));
        installments[3].DueDate.Should().Be(firstPayment.AddDays(21));
    }

    [Fact]
    public void Create_WithMonthlyFrequency_ShouldSpaceInstallmentsMonthly()
    {
        var firstPayment = new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        var arrangement = PaymentArrangement.Create(
            Guid.NewGuid(), Guid.NewGuid(), 300m, 3, 100m,
            PaymentFrequency.Monthly, firstPayment);

        var installments = arrangement.Installments.ToList();
        installments[0].DueDate.Should().Be(firstPayment);
        installments[1].DueDate.Should().Be(firstPayment.AddMonths(1));
        installments[2].DueDate.Should().Be(firstPayment.AddMonths(2));
    }

    [Fact]
    public void Activate_ShouldTransitionToActive()
    {
        var arrangement = CreateArrangement(PaymentFrequency.Monthly);

        arrangement.Activate();

        arrangement.Status.Should().Be(ArrangementStatus.Active);
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ShouldThrow()
    {
        var arrangement = CreateArrangement(PaymentFrequency.Monthly);
        arrangement.Activate();

        var act = () => arrangement.Activate();
        act.Should().Throw<InvalidCollectionStateException>();
    }

    [Fact]
    public void RecordPayment_OnActiveArrangement_ShouldRecord()
    {
        var arrangement = CreateArrangement(PaymentFrequency.Monthly);
        arrangement.Activate();

        arrangement.RecordPayment(100m);

        arrangement.PaidAmount.Should().Be(100m);
        arrangement.Installments.First().Status.Should().Be(InstallmentStatus.Paid);
        arrangement.LastPaymentDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void RecordPayment_FullAmount_ShouldComplete()
    {
        var arrangement = CreateArrangement(PaymentFrequency.Monthly);
        arrangement.Activate();

        arrangement.RecordPayment(600m);

        arrangement.Status.Should().Be(ArrangementStatus.Completed);
        arrangement.PaidAmount.Should().Be(600m);
    }

    [Fact]
    public void RecordPayment_WhenNotActive_ShouldThrow()
    {
        var arrangement = CreateArrangement(PaymentFrequency.Monthly);

        var act = () => arrangement.RecordPayment(100m);
        act.Should().Throw<InvalidCollectionStateException>().WithMessage("*Cannot record payment on a non-active arrangement*");
    }

    [Fact]
    public void RecordPayment_WithNegativeAmount_ShouldThrow()
    {
        var arrangement = CreateArrangement(PaymentFrequency.Monthly);
        arrangement.Activate();

        var act = () => arrangement.RecordPayment(-50m);
        act.Should().Throw<InvalidCollectionStateException>().WithMessage("*Payment amount must be positive*");
    }

    [Fact]
    public void RecordPayment_ExceedingRemaining_ShouldThrow()
    {
        var arrangement = CreateArrangement(PaymentFrequency.Monthly);
        arrangement.Activate();

        var act = () => arrangement.RecordPayment(1000m);
        act.Should().Throw<InvalidCollectionStateException>().WithMessage("*exceeds remaining balance*");
    }

    [Fact]
    public void Default_ShouldTransitionAndMarkInstallments()
    {
        var arrangement = CreateArrangement(PaymentFrequency.Monthly);
        arrangement.Activate();
        arrangement.RecordPayment(100m);

        arrangement.Default();

        arrangement.Status.Should().Be(ArrangementStatus.Defaulted);
        arrangement.DefaultedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        arrangement.Installments.Skip(1).Should().AllSatisfy(i =>
            i.Status.Should().Be(InstallmentStatus.Defaulted));
    }

    [Fact]
    public void Default_ShouldRaiseEvent()
    {
        var arrangement = CreateArrangement(PaymentFrequency.Monthly);
        arrangement.Activate();

        arrangement.Default();

        arrangement.DomainEvents.Should().Contain(e => e is PaymentArrangementDefaultedDomainEvent);
    }

    [Fact]
    public void Default_WhenNotActive_ShouldThrow()
    {
        var arrangement = CreateArrangement(PaymentFrequency.Monthly);

        var act = () => arrangement.Default();
        act.Should().Throw<InvalidCollectionStateException>();
    }

    [Fact]
    public void Complete_WhenActive_ShouldSucceed()
    {
        var arrangement = CreateArrangement(PaymentFrequency.Monthly);
        arrangement.Activate();

        arrangement.Complete();

        arrangement.Status.Should().Be(ArrangementStatus.Completed);
    }

    [Fact]
    public void Complete_WhenNotActive_ShouldThrow()
    {
        var arrangement = CreateArrangement(PaymentFrequency.Monthly);

        var act = () => arrangement.Complete();
        act.Should().Throw<InvalidCollectionStateException>();
    }

    [Fact]
    public void Cancel_FromProposed_ShouldSucceed()
    {
        var arrangement = CreateArrangement(PaymentFrequency.Monthly);

        arrangement.Cancel();

        arrangement.Status.Should().Be(ArrangementStatus.Cancelled);
    }

    [Fact]
    public void Cancel_FromActive_ShouldSucceed()
    {
        var arrangement = CreateArrangement(PaymentFrequency.Monthly);
        arrangement.Activate();

        arrangement.Cancel();

        arrangement.Status.Should().Be(ArrangementStatus.Cancelled);
    }

    [Fact]
    public void Cancel_FromCompleted_ShouldThrow()
    {
        var arrangement = CreateArrangement(PaymentFrequency.Monthly);
        arrangement.Activate();
        arrangement.Complete();

        var act = () => arrangement.Cancel();
        act.Should().Throw<InvalidCollectionStateException>();
    }

    [Fact]
    public void Cancel_FromCancelled_ShouldThrow()
    {
        var arrangement = CreateArrangement(PaymentFrequency.Monthly);
        arrangement.Cancel();

        var act = () => arrangement.Cancel();
        act.Should().Throw<InvalidCollectionStateException>();
    }

    [Fact]
    public void MarkOverdueInstallments_ShouldMarkPastDueInstallments()
    {
        var pastDate = DateTime.UtcNow.AddDays(-10);
        var arrangement = PaymentArrangement.Create(
            Guid.NewGuid(), Guid.NewGuid(), 200m, 2, 100m,
            PaymentFrequency.Monthly, pastDate);
        arrangement.Activate();

        arrangement.MarkOverdueInstallments();

        arrangement.Installments.First().Status.Should().Be(InstallmentStatus.Overdue);
    }

    private static PaymentArrangement CreateArrangement(PaymentFrequency frequency)
    {
        return PaymentArrangement.Create(
            Guid.NewGuid(), Guid.NewGuid(), 600m, 6, 100m,
            frequency, DateTime.UtcNow.AddDays(7));
    }
}