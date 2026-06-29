using Xunit;
using FluentAssertions;
using Obss.ProductCatalog.Domain.Domain.Entities;
using Obss.ProductCatalog.Domain.Domain.ValueObjects;
using Obss.ProductCatalog.Infrastructure.Persistence.Repositories;

namespace Obss.ProductCatalog.Tests;

public class RepositoryTests : CatalogIntegrationTests
{
    [Fact]
    public async Task CanAddAndRetrieveProduct()
    {
        using var context = CreateDbContext();
        var repository = new ProductRepository(context);

        var tenantId = Guid.NewGuid().ToString("N");
        var product = Product.Create(
            tenantId,
            "Fiber Internet",
            "High-speed fiber optic internet",
            null,
            ProductType.Service,
            false,
            true,
            "Services");

        await repository.AddAsync(product);
        await context.SaveChangesAsync();

        var retrieved = await repository.GetByIdAsync(product.Id);

        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Fiber Internet");
        retrieved.Description.Should().Be("High-speed fiber optic internet");
        retrieved.ProductType.Should().Be(ProductType.Service);
        retrieved.IsShippable.Should().BeFalse();
        retrieved.Taxable.Should().BeTrue();
        retrieved.TaxCategory.Should().Be("Services");
        retrieved.LifecycleStatus.Should().Be(LifecycleStatus.Draft);
        retrieved.IsActive.Should().BeTrue();
        retrieved.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public async Task CanQueryProductsByType()
    {
        var tenantId = Guid.NewGuid().ToString("N");

        using (var context = CreateDbContext())
        {
            var repo = new ProductRepository(context);

            var physical = Product.Create(tenantId, "Monitor", null, null, ProductType.Physical, true, true, "Electronics");
            var digital = Product.Create(tenantId, "E-Book", null, null, ProductType.Digital, false, false, null);
            var service = Product.Create(tenantId, "Consulting", null, null, ProductType.Service, false, true, "Professional");

            await repo.AddAsync(physical);
            await repo.AddAsync(digital);
            await repo.AddAsync(service);
            await context.SaveChangesAsync();
        }

        using (var context = CreateDbContext())
        {
            var repo = new ProductRepository(context);
            var digitalProducts = await repo.GetFilteredAsync(null, ProductType.Digital, null, null, 1, 10);

            digitalProducts.Should().HaveCount(1);
            digitalProducts.Should().Contain(p => p.Name == "E-Book");
        }
    }

    [Fact]
    public async Task CanFilterProductsByLifecycleStatus()
    {
        using var context = CreateDbContext();
        var repo = new ProductRepository(context);
        var tenantId = Guid.NewGuid().ToString("N");

        var draft = Product.Create(tenantId, "Draft Product", null, null, ProductType.Service, false, false, null);
        var active = Product.Create(tenantId, "Active Product", null, null, ProductType.Physical, true, true, "Hardware");
        active.Activate();

        await repo.AddAsync(draft);
        await repo.AddAsync(active);
        await context.SaveChangesAsync();

        var activeResults = await repo.GetFilteredAsync(null, null, LifecycleStatus.Active, null, 1, 10);
        activeResults.Should().Contain(p => p.Name == "Active Product");
        activeResults.Should().NotContain(p => p.Name == "Draft Product");
    }

    [Fact]
    public async Task CanAddAndRetrieveOffer()
    {
        using var context = CreateDbContext();
        var repository = new OfferRepository(context);

        var tenantId = Guid.NewGuid().ToString("N");
        var offer = Offer.Create(
            tenantId,
            "Annual Plan",
            "Yearly subscription",
            OfferType.Recurring,
            true,
            12,
            BillingPeriod.Annual,
            true,
            5,
            DateTime.UtcNow,
            DateTime.UtcNow.AddYears(1));

        await repository.AddAsync(offer);
        await context.SaveChangesAsync();

        var retrieved = await repository.GetByIdAsync(offer.Id);

        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Annual Plan");
        retrieved.Description.Should().Be("Yearly subscription");
        retrieved.OfferType.Should().Be(OfferType.Recurring);
        retrieved.IsContract.Should().BeTrue();
        retrieved.ContractDurationMonths.Should().Be(12);
        retrieved.BillingPeriod.Should().Be(BillingPeriod.Annual);
        retrieved.TaxInclusive.Should().BeTrue();
        retrieved.SortOrder.Should().Be(5);
        retrieved.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CanGetActiveOffers()
    {
        using var context = CreateDbContext();
        var repository = new OfferRepository(context);
        var tenantId = Guid.NewGuid().ToString("N");

        var activeOffer = Offer.Create(
            tenantId, "Active Offer", null, OfferType.OneTime, false, null, null, false, 1, null, null);
        var inactiveOffer = Offer.Create(
            tenantId, "Inactive Offer", null, OfferType.Recurring, false, null, null, false, 2, null, null);
        inactiveOffer.Deactivate();

        await repository.AddAsync(activeOffer);
        await repository.AddAsync(inactiveOffer);
        await context.SaveChangesAsync();

        var activeOffers = await repository.GetActiveOffersAsync(null);

        activeOffers.Should().Contain(o => o.Name == "Active Offer");
        activeOffers.Should().NotContain(o => o.Name == "Inactive Offer");
    }

    [Fact]
    public async Task CanAddPricingToOffer()
    {
        using var context = CreateDbContext();
        var repository = new OfferRepository(context);
        var tenantId = Guid.NewGuid().ToString("N");

        var offer = Offer.Create(
            tenantId, "Tiered Plan", null, OfferType.UsageBased, false, null, BillingPeriod.Monthly, false, 1, null, null);

        var pricing = new OfferPricing(
            Guid.NewGuid(),
            offer.Id,
            PricingType.Tiered,
            "USD",
            0m,
            0m,
            0.05m,
            "MB",
            100,
            10000,
            true);

        offer.AddPricing(pricing);
        await repository.AddAsync(offer);
        await context.SaveChangesAsync();

        var retrieved = await repository.GetByIdWithPricingsAsync(offer.Id);

        retrieved.Should().NotBeNull();
        retrieved!.OfferPricings.Should().HaveCount(1);
        retrieved.OfferPricings.Should().Contain(p => p.PricingType == PricingType.Tiered);
        retrieved.OfferPricings.Should().Contain(p => p.Currency == "USD");
    }

    [Fact]
    public async Task CanAddAndRetrieveCategory()
    {
        using var context = CreateDbContext();
        var repository = new CategoryRepository(context);

        var tenantId = Guid.NewGuid().ToString("N");
        var category = Category.Create(
            tenantId,
            "Electronics",
            "Electronic devices and accessories",
            null,
            1);

        await repository.AddAsync(category);
        await context.SaveChangesAsync();

        var retrieved = await repository.GetByIdAsync(category.Id);

        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Electronics");
        retrieved.Description.Should().Be("Electronic devices and accessories");
        retrieved.SortOrder.Should().Be(1);
        retrieved.IsActive.Should().BeTrue();
        retrieved.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public async Task CanQueryActiveCategories()
    {
        using var context = CreateDbContext();
        var repository = new CategoryRepository(context);
        var tenantId = Guid.NewGuid().ToString("N");

        var activeCategory = Category.Create(tenantId, "Active Cat", "Active", null, 1);
        var inactiveCategory = Category.Create(tenantId, "Inactive Cat", "Inactive", null, 2);
        inactiveCategory.Deactivate();

        await repository.AddAsync(activeCategory);
        await repository.AddAsync(inactiveCategory);
        await context.SaveChangesAsync();

        var activeCategories = await repository.GetActiveCategoriesAsync();

        activeCategories.Should().Contain(c => c.Name == "Active Cat");
        activeCategories.Should().NotContain(c => c.Name == "Inactive Cat");
    }
}
