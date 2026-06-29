using Xunit;
using FluentAssertions;
using Obss.Billing.Domain.Entities;
using Obss.Billing.Domain.ValueObjects;

namespace Obss.Billing.Domain.Tests;

public class BillLineTests
{
    [Fact]
    public void CreateRecurring_ShouldSetProperties()
    {
        var billId = Guid.NewGuid();
        var subId = Guid.NewGuid();
        var lineDate = DateTime.UtcNow;

        var line = BillLine.CreateRecurring(
            billId, "Monthly Fee", subId, null, null,
            1, 99.99m, 0, 0.05m, "USD", lineDate);

        line.BillId.Should().Be(billId);
        line.LineType.Should().Be(LineType.Recurring);
        line.Description.Should().Be("Monthly Fee");
        line.SubscriptionId.Should().Be(subId);
        line.Quantity.Should().Be(1);
        line.UnitPrice.Should().Be(99.99m);
        line.TaxRate.Should().Be(0.05m);
        line.Currency.Should().Be("USD");
        line.Reference.Should().BeNull();
    }

    [Fact]
    public void CreateUsage_ShouldSetUsageReference()
    {
        var billId = Guid.NewGuid();
        var lineDate = DateTime.UtcNow;

        var line = BillLine.CreateUsage(
            billId, "API Calls", Guid.NewGuid(), null, null,
            1000, 0.01m, 0, 0m, "USD", lineDate, "usage-ref-123");

        line.LineType.Should().Be(LineType.Usage);
        line.Quantity.Should().Be(1000);
        line.UnitPrice.Should().Be(0.01m);
        line.Reference.Should().Be("usage-ref-123");
    }

    [Fact]
    public void CreateAdjustment_ShouldSetAmount()
    {
        var billId = Guid.NewGuid();
        var lineDate = DateTime.UtcNow;

        var line = BillLine.CreateAdjustment(
            billId, "Credit", -50m, "USD", lineDate);

        line.LineType.Should().Be(LineType.Adjustment);
        line.Description.Should().Be("Credit");
        line.UnitPrice.Should().Be(-50m);
        line.Quantity.Should().Be(1);
        line.DiscountAmount.Should().Be(0);
    }

    [Fact]
    public void CreateTaxLine_ShouldSetTaxRate()
    {
        var billId = Guid.NewGuid();
        var lineDate = DateTime.UtcNow;

        var line = BillLine.CreateTaxLine(
            billId, "VAT 15%", 15m, 0.15m, "USD", lineDate);

        line.LineType.Should().Be(LineType.Tax);
        line.Description.Should().Be("VAT 15%");
        line.UnitPrice.Should().Be(15m);
        line.TaxRate.Should().Be(0.15m);
    }

    [Fact]
    public void CalculateTotal_ShouldComputeCorrectly()
    {
        var billId = Guid.NewGuid();
        var lineDate = DateTime.UtcNow;
        var line = BillLine.CreateRecurring(
            billId, "Service", Guid.NewGuid(), null, null,
            3, 100m, 20m, 0.1m, "USD", lineDate);

        line.CalculateTotal();

        var subtotal = 3 * 100m;
        var afterDiscount = subtotal - 20m;
        var tax = afterDiscount * 0.1m;
        var total = afterDiscount + tax;

        line.TaxAmount.Should().Be(tax);
        line.TotalAmount.Should().Be(total);
    }
}
