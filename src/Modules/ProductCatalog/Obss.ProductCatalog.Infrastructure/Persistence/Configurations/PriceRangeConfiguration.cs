using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.ProductCatalog.Domain.Domain.Entities;

namespace Obss.ProductCatalog.Infrastructure.Persistence.Configurations;

public sealed class PriceRangeConfiguration : IEntityTypeConfiguration<PriceRange>
{
    public void Configure(EntityTypeBuilder<PriceRange> builder)
    {
        builder.ToTable("price_ranges");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();

        builder.Property(r => r.OfferPricingId).HasColumnName("offer_pricing_id").IsRequired();
        builder.Property(r => r.MinQuantity).HasColumnName("min_quantity").IsRequired();
        builder.Property(r => r.MaxQuantity).HasColumnName("max_quantity");
        builder.Property(r => r.Price).HasColumnName("price").HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(r => r.IsActive).HasColumnName("is_active").IsRequired();

        builder.HasIndex(r => r.OfferPricingId).HasDatabaseName("ix_price_ranges_offer_pricing_id");
    }
}
