using Xunit;
using FluentAssertions;
using Obss.Invoices.Domain.Entities;
using Obss.Invoices.Domain.ValueObjects;

namespace Obss.Invoices.Tests.Domain;

public class InvoiceLineTests
{
    [Fact]
    public void OneTimeLine_ShouldSetProperties()
    {
        var line = new InvoiceLine(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null,
            LineType.OneTime, "Setup fee", 1, 200m, 200m, 20m, 0.10m, "USD");

        line.LineType.Should().Be(LineType.OneTime);
        line.Description.Should().Be("Setup fee");
        line.Quantity.Should().Be(1);
        line.UnitPrice.Should().Be(200m);
        line.TotalAmount.Should().Be(200m);
        line.TaxAmount.Should().Be(20m);
        line.TaxRate.Should().Be(0.10m);
    }

    [Fact]
    public void RecurringLine_ShouldSetProperties()
    {
        var line = new InvoiceLine(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null,
            LineType.Recurring, "Monthly subscription", 1, 49.99m, 49.99m, 5m, 0.10m, "USD");

        line.LineType.Should().Be(LineType.Recurring);
        line.TotalAmount.Should().Be(49.99m);
    }

    [Fact]
    public void UsageLine_ShouldSetProperties()
    {
        var line = new InvoiceLine(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null,
            LineType.Usage, "API calls", 1000, 0.01m, 10m, 1m, 0.10m, "USD");

        line.LineType.Should().Be(LineType.Usage);
        line.Quantity.Should().Be(1000);
        line.UnitPrice.Should().Be(0.01m);
        line.TotalAmount.Should().Be(10m);
    }

    [Fact]
    public void AdjustmentLine_ShouldSetProperties()
    {
        var line = new InvoiceLine(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null,
            LineType.Adjustment, "Credit adjustment", 1, -50m, -50m, 0m, 0m, "USD");

        line.LineType.Should().Be(LineType.Adjustment);
        line.TotalAmount.Should().Be(-50m);
    }

    [Fact]
    public void LineWithBillLineId_ShouldHaveReference()
    {
        var billLineId = Guid.NewGuid();
        var line = new InvoiceLine(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), billLineId,
            LineType.OneTime, "Referenced line", 1, 100m, 100m, 10m, 0.10m, "USD");

        line.BillLineId.Should().Be(billLineId);
    }

    [Fact]
    public void LineWithoutBillLineId_ShouldBeNull()
    {
        var line = new InvoiceLine(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null,
            LineType.OneTime, "No reference", 1, 100m, 100m, 10m, 0.10m, "USD");

        line.BillLineId.Should().BeNull();
    }
}
