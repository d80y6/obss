using Xunit;
using FluentAssertions;
using Obss.Orders.Domain.Entities;
using Obss.Orders.Domain.ValueObjects;

namespace Obss.Orders.Tests.Domain;

public class OrderItemTests
{
    private static Order CreateDraftOrder()
    {
        return Order.Create(
            "tenant-1", Guid.NewGuid(), "John Doe",
            OrderType.New, "user-1");
    }

    [Fact]
    public void AddedItem_ShouldHaveCorrectProperties()
    {
        var order = CreateDraftOrder();
        var productId = Guid.NewGuid();
        var offerId = Guid.NewGuid();

        order.AddItem(
            productId, offerId,
            "Internet Plan", "Basic 100Mbps",
            2, 49.99m, 29.99m, 5m, 8m,
            BillingPeriod.Monthly,
            new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc));

        var item = order.Items.Single();
        item.OrderId.Should().Be(order.Id);
        item.ProductId.Should().Be(productId);
        item.OfferId.Should().Be(offerId);
        item.ProductName.Should().Be("Internet Plan");
        item.OfferName.Should().Be("Basic 100Mbps");
        item.Quantity.Should().Be(2);
        item.UnitPrice.Should().Be(49.99m);
        item.RecurringPrice.Should().Be(29.99m);
        item.DiscountAmount.Should().Be(5m);
        item.TaxAmount.Should().Be(8m);
        item.BillingPeriod.Should().Be(BillingPeriod.Monthly);
        item.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        var order = CreateDraftOrder();
        order.AddItem(
            Guid.NewGuid(), Guid.NewGuid(),
            "Product", "Offer", 1, 10m, 0, 0, 0,
            BillingPeriod.Monthly);
        var item = order.Items.Single();

        item.Deactivate();

        item.IsActive.Should().BeFalse();
    }

    [Fact]
    public void UpdateQuantity_ShouldChangeQuantity()
    {
        var order = CreateDraftOrder();
        order.AddItem(
            Guid.NewGuid(), Guid.NewGuid(),
            "Product", "Offer", 1, 10m, 0, 0, 0,
            BillingPeriod.Monthly);
        var item = order.Items.Single();

        item.UpdateQuantity(5);

        item.Quantity.Should().Be(5);
    }

    [Fact]
    public void UpdateQuantity_WhenZeroOrNegative_ShouldThrow()
    {
        var order = CreateDraftOrder();
        order.AddItem(
            Guid.NewGuid(), Guid.NewGuid(),
            "Product", "Offer", 1, 10m, 0, 0, 0,
            BillingPeriod.Monthly);
        var item = order.Items.Single();

        var act = () => item.UpdateQuantity(0);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Quantity must be greater than zero*");
    }
}
