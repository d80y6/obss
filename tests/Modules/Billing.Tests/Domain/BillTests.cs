using Xunit;
using FluentAssertions;
using Obss.Billing.Domain.Entities;
using Obss.Billing.Domain.Exceptions;
using Obss.Billing.Domain.ValueObjects;

namespace Obss.Billing.Domain.Tests;

public class BillTests
{
    [Fact]
    public void Create_ShouldSetPropertiesCorrectly()
    {
        var customerId = Guid.NewGuid();
        var periodStart = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var periodEnd = new DateTime(2025, 1, 31, 0, 0, 0, DateTimeKind.Utc);
        var dueDate = new DateTime(2025, 2, 15, 0, 0, 0, DateTimeKind.Utc);

        var bill = Bill.Create(
            "tenant-1",
            customerId,
            "Customer A",
            BillingPeriod.Monthly,
            periodStart,
            periodEnd,
            dueDate,
            "USD");

        bill.Id.Should().NotBeEmpty();
        bill.TenantId.Should().Be("tenant-1");
        bill.CustomerId.Should().Be(customerId);
        bill.CustomerName.Should().Be("Customer A");
        bill.BillingPeriod.Should().Be(BillingPeriod.Monthly);
        bill.BillingPeriodStart.Should().Be(periodStart);
        bill.BillingPeriodEnd.Should().Be(periodEnd);
        bill.DueDate.Should().Be(dueDate);
        bill.Currency.Should().Be("USD");
        bill.Status.Should().Be(BillStatus.Draft);
        bill.SubTotal.Should().Be(0);
        bill.GrandTotal.Should().Be(0);
        bill.Lines.Should().BeEmpty();
    }

    [Fact]
    public void AddLine_WhenDraft_ShouldAddLine()
    {
        var bill = CreateDraftBill();
        var line = BillLine.CreateRecurring(
            bill.Id, "Subscription Fee", Guid.NewGuid(), null, null,
            1, 100m, 0, 0.05m, "USD", DateTime.UtcNow);

        bill.AddLine(line);

        bill.Lines.Should().ContainSingle();
        bill.Lines.First().Description.Should().Be("Subscription Fee");
    }

    [Fact]
    public void AddLine_WhenFinalized_ShouldThrow()
    {
        var bill = CreateDraftBill();
        var line = BillLine.CreateRecurring(
            bill.Id, "Test", Guid.NewGuid(), null, null,
            1, 50m, 0, 0m, "USD", DateTime.UtcNow);
        bill.AddLine(line);
        bill.CalculateTotals();
        bill.MarkAsFinalized();

        var anotherLine = BillLine.CreateRecurring(
            bill.Id, "Should Fail", Guid.NewGuid(), null, null,
            1, 10m, 0, 0m, "USD", DateTime.UtcNow);

        var act = () => bill.AddLine(anotherLine);
        act.Should().Throw<InvalidBillStateException>()
            .WithMessage("*Cannot add lines to a finalized bill.*");
    }

    [Fact]
    public void CalculateTotals_ShouldSetCalculatedStatus()
    {
        var bill = CreateDraftBill();
        var line = BillLine.CreateRecurring(
            bill.Id, "Item", Guid.NewGuid(), null, null,
            2, 50m, 10m, 0.1m, "USD", DateTime.UtcNow);
        bill.AddLine(line);

        bill.CalculateTotals();

        bill.Status.Should().Be(BillStatus.Calculated);
        bill.SubTotal.Should().Be(100m);
        bill.DiscountTotal.Should().Be(10m);
        bill.TaxTotal.Should().Be(9m);
        bill.GrandTotal.Should().Be(99m);
    }

    [Fact]
    public void MarkAsFinalized_WhenCalculated_ShouldSucceed()
    {
        var bill = CreateDraftBill();
        bill.AddLine(BillLine.CreateRecurring(
            bill.Id, "Item", Guid.NewGuid(), null, null,
            1, 100m, 0, 0m, "USD", DateTime.UtcNow));
        bill.CalculateTotals();

        bill.MarkAsFinalized();

        bill.Status.Should().Be(BillStatus.Finalized);
        bill.FinalizedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void MarkAsFinalized_WhenNotCalculated_ShouldThrow()
    {
        var bill = CreateDraftBill();

        var act = () => bill.MarkAsFinalized();
        act.Should().Throw<InvalidBillStateException>();
    }

    [Fact]
    public void MarkAsInvoiced_WhenFinalized_ShouldSucceed()
    {
        var bill = CreateFinalizedBill();

        bill.MarkAsInvoiced();

        bill.Status.Should().Be(BillStatus.Invoiced);
    }

    [Fact]
    public void MarkAsInvoiced_WhenNotFinalized_ShouldThrow()
    {
        var bill = CreateDraftBill();

        var act = () => bill.MarkAsInvoiced();
        act.Should().Throw<InvalidBillStateException>();
    }

    [Fact]
    public void Close_WhenInvoiced_ShouldSucceed()
    {
        var bill = CreateFinalizedBill();
        bill.MarkAsInvoiced();

        bill.Close();

        bill.Status.Should().Be(BillStatus.Closed);
    }

    [Fact]
    public void Close_WhenNotInvoiced_ShouldThrow()
    {
        var bill = CreateDraftBill();

        var act = () => bill.Close();
        act.Should().Throw<InvalidBillStateException>();
    }

    [Fact]
    public void AddTaxLine_WhenFinalized_ShouldThrow()
    {
        var bill = CreateFinalizedBill();
        var taxLine = BillLine.CreateTaxLine(
            bill.Id, "VAT", 10m, 0.1m, "USD", DateTime.UtcNow);

        var act = () => bill.AddTaxLine(taxLine);
        act.Should().Throw<InvalidBillStateException>();
    }

    [Fact]
    public void AddTaxLine_WhenDraft_ShouldAdd()
    {
        var bill = CreateDraftBill();
        var taxLine = BillLine.CreateTaxLine(
            bill.Id, "VAT", 10m, 0.1m, "USD", DateTime.UtcNow);

        bill.AddTaxLine(taxLine);

        bill.Lines.Should().ContainSingle();
    }

    [Fact]
    public void RecalculateTotalsWithTax_ShouldComputeCorrectly()
    {
        var bill = CreateDraftBill();
        bill.AddLine(BillLine.CreateRecurring(
            bill.Id, "Service", Guid.NewGuid(), null, null,
            1, 200m, 20m, 0m, "USD", DateTime.UtcNow));
        bill.AddTaxLine(BillLine.CreateTaxLine(
            bill.Id, "VAT 10%", 18m, 0.1m, "USD", DateTime.UtcNow));

        bill.RecalculateTotalsWithTax();

        bill.SubTotal.Should().Be(200m);
        bill.DiscountTotal.Should().Be(20m);
        bill.TaxTotal.Should().Be(18m);
        bill.GrandTotal.Should().Be(198m);
    }

    [Fact]
    public void RecalculateTotalsWithTax_WhenFinalized_ShouldThrow()
    {
        var bill = CreateFinalizedBill();

        var act = () => bill.RecalculateTotalsWithTax();
        act.Should().Throw<InvalidBillStateException>();
    }

    private static Bill CreateDraftBill()
    {
        return Bill.Create(
            "tenant-1",
            Guid.NewGuid(),
            "Customer A",
            BillingPeriod.Monthly,
            DateTime.UtcNow.AddMonths(-1),
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(15),
            "USD");
    }

    private static Bill CreateFinalizedBill()
    {
        var bill = CreateDraftBill();
        bill.AddLine(BillLine.CreateRecurring(
            bill.Id, "Item", Guid.NewGuid(), null, null,
            1, 100m, 0, 0m, "USD", DateTime.UtcNow));
        bill.CalculateTotals();
        bill.MarkAsFinalized();
        return bill;
    }
}
