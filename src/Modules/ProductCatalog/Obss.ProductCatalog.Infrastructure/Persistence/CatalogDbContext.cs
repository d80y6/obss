using Microsoft.EntityFrameworkCore;
using Obss.ProductCatalog.Domain.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.ProductCatalog.Infrastructure.Persistence;

public class CatalogDbContext : EfDbContext
{
    public CatalogDbContext(
        DbContextOptions<CatalogDbContext> options,
        ICurrentTenant currentTenant,
        IDomainEventDispatcher domainEventDispatcher)
        : base(options, currentTenant, domainEventDispatcher)
    {
    }

    public DbSet<Catalog> Catalogs => Set<Catalog>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Offer> Offers => Set<Offer>();
    public DbSet<OfferPricing> OfferPricings => Set<OfferPricing>();
    public DbSet<ProductOffer> ProductOffers => Set<ProductOffer>();
    public DbSet<OfferDiscount> OfferDiscounts => Set<OfferDiscount>();
    public DbSet<ProductConfigurationRule> ProductConfigurationRules => Set<ProductConfigurationRule>();
    public DbSet<ProductOption> ProductOptions => Set<ProductOption>();
    public DbSet<ProductSpecification> ProductSpecifications => Set<ProductSpecification>();
    public DbSet<OptionValue> OptionValues => Set<OptionValue>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
