using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.ProductCatalog.Domain.Domain.Entities;

namespace Obss.ProductCatalog.Infrastructure.Persistence.Configurations;

public sealed class OfferPricingConfiguration : IEntityTypeConfiguration<OfferPricing>
{
    public void Configure(EntityTypeBuilder<OfferPricing> builder)
    {
        builder.ToTable("offer_pricings");

        builder.HasKey(op => op.Id);

        builder.Property(op => op.Id)
            .ValueGeneratedNever();

        builder.Property(op => op.OfferId)
            .HasColumnName("offer_id")
            .IsRequired();

        builder.Property(op => op.PricingType)
            .HasColumnName("pricing_type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(op => op.Currency)
            .HasColumnName("currency")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(op => op.RecurringPrice)
            .HasColumnName("recurring_price")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(op => op.OneTimePrice)
            .HasColumnName("one_time_price")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(op => op.UsagePrice)
            .HasColumnName("usage_price")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(op => op.UnitOfMeasure)
            .HasColumnName("unit_of_measure")
            .HasMaxLength(50);

        builder.Property(op => op.MinQuantity)
            .HasColumnName("min_quantity");

        builder.Property(op => op.MaxQuantity)
            .HasColumnName("max_quantity");

        builder.Property(op => op.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(op => op.Name)
            .HasColumnName("name")
            .HasMaxLength(200);

        builder.Property(op => op.Description)
            .HasColumnName("description")
            .HasMaxLength(2000);

        builder.Property(op => op.PriceApplicationType)
            .HasColumnName("price_application_type")
            .HasConversion<string?>()
            .HasMaxLength(50);

        builder.HasMany(op => op.PriceRanges)
            .WithOne()
            .HasForeignKey(pr => pr.OfferPricingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(op => op.OfferId).HasDatabaseName("ix_offer_pricings_offer_id");
    }
}
