using Xunit;
using FluentAssertions;
using Obss.Billing.Domain.Entities;
using Obss.Billing.Domain.ValueObjects;

namespace Obss.Billing.Domain.Tests;

public class TaxRuleTests
{
    [Fact]
    public void Create_ShouldSetProperties()
    {
        var effectiveFrom = DateTime.UtcNow.AddDays(-30);

        var rule = TaxRule.Create(
            "tenant-1",
            "VAT Standard",
            "Standard VAT rate",
            0.15m,
            TaxType.Percentage,
            "goods",
            "YE",
            "Sana'a",
            false,
            1,
            effectiveFrom,
            null);

        rule.Id.Should().NotBeEmpty();
        rule.TenantId.Should().Be("tenant-1");
        rule.Name.Should().Be("VAT Standard");
        rule.TaxRate.Should().Be(0.15m);
        rule.TaxType.Should().Be(TaxType.Percentage);
        rule.TaxCategory.Should().Be("goods");
        rule.Country.Should().Be("YE");
        rule.IsCompound.Should().BeFalse();
        rule.IsActive.Should().BeTrue();
        rule.Priority.Should().Be(1);
    }

    [Fact]
    public void IsApplicable_WhenActiveAndInRange_ShouldReturnTrue()
    {
        var rule = TaxRule.Create(
            "tenant-1", "VAT", "desc", 0.15m,
            TaxType.Percentage, "goods", "YE", "", false, 1,
            DateTime.UtcNow.AddDays(-30), null);

        var result = rule.IsApplicable("goods", "YE");

        result.Should().BeTrue();
    }

    [Fact]
    public void IsApplicable_WhenInactive_ShouldReturnFalse()
    {
        var rule = TaxRule.Create(
            "tenant-1", "VAT", "desc", 0.15m,
            TaxType.Percentage, "goods", "YE", "", false, 1,
            DateTime.UtcNow.AddDays(-30), null);
        rule.Deactivate();

        var result = rule.IsApplicable("goods", "YE");

        result.Should().BeFalse();
    }

    [Fact]
    public void IsApplicable_WhenCategoryDoesNotMatch_ShouldReturnFalse()
    {
        var rule = TaxRule.Create(
            "tenant-1", "VAT", "desc", 0.15m,
            TaxType.Percentage, "goods", "YE", "", false, 1,
            DateTime.UtcNow.AddDays(-30), null);

        var result = rule.IsApplicable("services", "YE");

        result.Should().BeFalse();
    }

    [Fact]
    public void IsApplicable_WhenBeforeEffectiveFrom_ShouldReturnFalse()
    {
        var rule = TaxRule.Create(
            "tenant-1", "VAT", "desc", 0.15m,
            TaxType.Percentage, "goods", "YE", "", false, 1,
            DateTime.UtcNow.AddDays(30), null);

        var result = rule.IsApplicable("goods", "YE");

        result.Should().BeFalse();
    }

    [Fact]
    public void IsApplicable_WhenAfterEffectiveTo_ShouldReturnFalse()
    {
        var rule = TaxRule.Create(
            "tenant-1", "VAT", "desc", 0.15m,
            TaxType.Percentage, "goods", "YE", "", false, 1,
            DateTime.UtcNow.AddDays(-60),
            DateTime.UtcNow.AddDays(-30));

        var result = rule.IsApplicable("goods", "YE");

        result.Should().BeFalse();
    }

    [Fact]
    public void Activate_ShouldSetIsActiveTrue()
    {
        var rule = TaxRule.Create(
            "tenant-1", "VAT", "desc", 0.15m,
            TaxType.Percentage, "goods", "YE", "", false, 1,
            DateTime.UtcNow.AddDays(-30), null);
        rule.Deactivate();

        rule.Activate();

        rule.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveFalse()
    {
        var rule = TaxRule.Create(
            "tenant-1", "VAT", "desc", 0.15m,
            TaxType.Percentage, "goods", "YE", "", false, 1,
            DateTime.UtcNow.AddDays(-30), null);

        rule.Deactivate();

        rule.IsActive.Should().BeFalse();
    }

    [Fact]
    public void CalculateTax_Percentage_ShouldMultiplyAmount()
    {
        var rule = TaxRule.Create(
            "tenant-1", "VAT", "desc", 0.15m,
            TaxType.Percentage, "goods", "YE", "", false, 1,
            DateTime.UtcNow.AddDays(-30), null);

        var tax = rule.CalculateTax(200m);

        tax.Should().Be(30m);
    }

    [Fact]
    public void CalculateTax_Fixed_ShouldReturnRateAsAmount()
    {
        var rule = TaxRule.Create(
            "tenant-1", "Fixed Tax", "desc", 10m,
            TaxType.Fixed, "services", "YE", "", false, 1,
            DateTime.UtcNow.AddDays(-30), null);

        var tax = rule.CalculateTax(200m);

        tax.Should().Be(10m);
    }

    [Fact]
    public void IsApplicable_WhenCountryDoesNotMatch_ShouldReturnFalse()
    {
        var rule = TaxRule.Create(
            "tenant-1", "VAT", "desc", 0.15m,
            TaxType.Percentage, "goods", "YE", "", false, 1,
            DateTime.UtcNow.AddDays(-30), null);

        var result = rule.IsApplicable("goods", "SA");

        result.Should().BeFalse();
    }
}
