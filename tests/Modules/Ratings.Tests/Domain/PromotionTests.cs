using Xunit;
using FluentAssertions;
using Obss.Rating.Domain.Entities;
using Obss.Rating.Domain.ValueObjects;

namespace Obss.Rating.Tests.Domain;

public class PromotionTests
{
    [Fact]
    public void Create_ShouldSetProperties()
    {
        var validFrom = DateTime.UtcNow.AddDays(-1);
        var validTo = DateTime.UtcNow.AddDays(30);

        var promotion = Promotion.Create(
            "tenant-1", "Summer Sale", "20% off", PromotionType.Percentage, 20m, "USD",
            null, null, validFrom, validTo, true, 1, "SUMMER20", 1000);

        promotion.Name.Should().Be("Summer Sale");
        promotion.PromotionType.Should().Be(PromotionType.Percentage);
        promotion.DiscountValue.Should().Be(20m);
        promotion.Currency.Should().Be("USD");
        promotion.IsActive.Should().BeTrue();
        promotion.ValidFrom.Should().Be(validFrom);
        promotion.ValidTo.Should().Be(validTo);
        promotion.Code.Should().Be("SUMMER20");
    }

    [Fact]
    public void IsValid_WhenActiveAndInDateRange_ShouldReturnTrue()
    {
        var promotion = CreateActivePromotion();
        promotion.IsValid().Should().BeTrue();
    }

    [Fact]
    public void IsValid_WhenDeactivated_ShouldReturnFalse()
    {
        var promotion = CreateActivePromotion();
        promotion.Deactivate();
        promotion.IsValid().Should().BeFalse();
    }

    [Fact]
    public void IsValid_WhenExpired_ShouldReturnFalse()
    {
        var promotion = Promotion.Create(
            "t1", "Old", null, PromotionType.Percentage, 10m, "USD",
            null, null, DateTime.UtcNow.AddDays(-60), DateTime.UtcNow.AddDays(-30),
            true, 1, null, null);

        promotion.IsValid().Should().BeFalse();
    }

    [Fact]
    public void IsValid_WhenMaxRedemptionsReached_ShouldReturnFalse()
    {
        var promotion = Promotion.Create(
            "t1", "Limited", null, PromotionType.FixedAmount, 10m, "USD",
            null, null, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(30),
            true, 1, null, 1);

        promotion.IncrementRedemptions();
        promotion.IsValid().Should().BeFalse();
    }

    [Fact]
    public void CalculateDiscount_Percentage_ShouldReturnCorrectAmount()
    {
        var promotion = Promotion.Create(
            "t1", "10% Off", null, PromotionType.Percentage, 10m, "USD",
            null, null, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(30),
            true, 1, null, null);

        var discount = promotion.CalculateDiscount(200m);
        discount.Should().Be(20m);
    }

    [Fact]
    public void CalculateDiscount_FixedAmount_ShouldReturnFixedValue()
    {
        var promotion = Promotion.Create(
            "t1", "$5 Off", null, PromotionType.FixedAmount, 5m, "USD",
            null, null, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(30),
            true, 1, null, null);

        var discount = promotion.CalculateDiscount(50m);
        discount.Should().Be(5m);
    }

    [Fact]
    public void IsApplicable_WhenQuantityBelowMinimum_ShouldReturnFalse()
    {
        var promotion = Promotion.Create(
            "t1", "Bulk Discount", null, PromotionType.Volume, 10m, "USD",
            5, null, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(30),
            true, 1, null, null);

        promotion.IsApplicable(100m, 2).Should().BeFalse();
        promotion.IsApplicable(100m, 5).Should().BeTrue();
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveFalse()
    {
        var promotion = CreateActivePromotion();
        promotion.Deactivate();
        promotion.IsActive.Should().BeFalse();
    }

    [Fact]
    public void IncrementRedemptions_ShouldIncreaseCount()
    {
        var promotion = CreateActivePromotion();
        promotion.CurrentRedemptions.Should().Be(0);
        promotion.IncrementRedemptions();
        promotion.CurrentRedemptions.Should().Be(1);
    }

    private static Promotion CreateActivePromotion()
    {
        return Promotion.Create(
            "t1", "Test Promotion", null, PromotionType.Percentage, 10m, "USD",
            null, null, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(30),
            true, 1, null, null);
    }
}
