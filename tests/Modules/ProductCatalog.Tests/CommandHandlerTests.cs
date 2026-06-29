using Xunit;
using FluentAssertions;
using NSubstitute;
using Obss.ProductCatalog.Application.Commands.CreateOffer;
using Obss.ProductCatalog.Application.Commands.CreateProduct;
using Obss.ProductCatalog.Domain.Domain.ValueObjects;
using Obss.ProductCatalog.Infrastructure.Persistence;
using Obss.ProductCatalog.Infrastructure.Persistence.Repositories;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.ProductCatalog.Tests;

public class CommandHandlerTests : CatalogIntegrationTests
{
    [Fact]
    public async Task CreateProductCommand_ShouldCreateProductInDatabase()
    {
        using var context = CreateDbContext();
        var productRepository = new ProductRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var handler = new CreateProductCommandHandler(productRepository, unitOfWork);

        var tenantId = Guid.NewGuid().ToString("N");
        var command = new CreateProductCommand(
            tenantId,
            "Basic Internet",
            "Basic home internet plan",
            null,
            ProductType.Service,
            false,
            true,
            "Services",
            null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("Basic Internet");
        result.Value.Description.Should().Be("Basic home internet plan");
        result.Value.ProductType.Should().Be(ProductType.Service);
        result.Value.IsShippable.Should().BeFalse();
        result.Value.Taxable.Should().BeTrue();
        result.Value.LifecycleStatus.Should().Be(LifecycleStatus.Draft);

        var saved = await productRepository.GetByIdAsync(result.Value.Id);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("Basic Internet");
    }

    [Fact]
    public async Task CreateProductCommand_ShouldCreatePhysicalProduct()
    {
        using var context = CreateDbContext();
        var productRepository = new ProductRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var handler = new CreateProductCommandHandler(productRepository, unitOfWork);

        var tenantId = Guid.NewGuid().ToString("N");
        var command = new CreateProductCommand(
            tenantId,
            "Laptop Pro",
            "High-performance laptop",
            null,
            ProductType.Physical,
            true,
            true,
            "Electronics",
            null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ProductType.Should().Be(ProductType.Physical);
        result.Value.IsShippable.Should().BeTrue();
        result.Value.TaxCategory.Should().Be("Electronics");
    }

    [Fact]
    public async Task CreateProductCommand_ShouldDefaultToDraftStatus()
    {
        using var context = CreateDbContext();
        var productRepository = new ProductRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var handler = new CreateProductCommandHandler(productRepository, unitOfWork);

        var tenantId = Guid.NewGuid().ToString("N");
        var command = new CreateProductCommand(
            tenantId,
            "Digital Album",
            null,
            null,
            ProductType.Digital,
            false,
            false,
            null,
            null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Value.LifecycleStatus.Should().Be(LifecycleStatus.Draft);
    }

    [Fact]
    public async Task CreateOfferCommand_ShouldCreateOfferInDatabase()
    {
        using var context = CreateDbContext();
        var offerRepository = new OfferRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var handler = new CreateOfferCommandHandler(offerRepository, unitOfWork);

        var tenantId = Guid.NewGuid().ToString("N");
        var command = new CreateOfferCommand(
            tenantId,
            "Setup Fee",
            "One-time setup fee",
            OfferType.OneTime,
            false,
            null,
            null,
            false,
            1,
            null,
            null,
            null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("Setup Fee");
        result.Value.Description.Should().Be("One-time setup fee");
        result.Value.OfferType.Should().Be(OfferType.OneTime);
        result.Value.IsContract.Should().BeFalse();
        result.Value.SortOrder.Should().Be(1);
        result.Value.IsActive.Should().BeTrue();

        var saved = await offerRepository.GetByIdAsync(result.Value.Id);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("Setup Fee");
    }

    [Fact]
    public async Task CreateOfferCommand_ShouldCreateRecurringOfferWithPricing()
    {
        using var context = CreateDbContext();
        var offerRepository = new OfferRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var handler = new CreateOfferCommandHandler(offerRepository, unitOfWork);

        var tenantId = Guid.NewGuid().ToString("N");
        var pricings = new List<CreateOfferPricingItem>
        {
            new(
                PricingType.Flat,
                "USD",
                49.99m,
                0m,
                0m,
                null,
                null,
                null,
                true)
        };

        var command = new CreateOfferCommand(
            tenantId,
            "Monthly Subscription",
            "Premium monthly plan",
            OfferType.Recurring,
            true,
            12,
            BillingPeriod.Monthly,
            true,
            10,
            DateTime.UtcNow,
            DateTime.UtcNow.AddYears(1),
            pricings);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.OfferType.Should().Be(OfferType.Recurring);
        result.Value.IsContract.Should().BeTrue();
        result.Value.ContractDurationMonths.Should().Be(12);
        result.Value.BillingPeriod.Should().Be(BillingPeriod.Monthly);
        result.Value.TaxInclusive.Should().BeTrue();
        result.Value.Pricings.Should().HaveCount(1);
        result.Value.Pricings[0].Currency.Should().Be("USD");
        result.Value.Pricings[0].RecurringPrice.Should().Be(49.99m);
    }
}
