using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.ProductCatalog.Domain.Domain.Entities;

namespace Obss.ProductCatalog.Infrastructure.Persistence.Configurations;

public sealed class OfferDiscountConfiguration : IEntityTypeConfiguration<OfferDiscount>
{
    public void Configure(EntityTypeBuilder<OfferDiscount> builder)
    {
        builder.ToTable("offer_discounts");

        builder.HasKey(od => od.Id);

        builder.Property(od => od.Id)
            .ValueGeneratedNever();

        builder.Property(od => od.OfferId)
            .HasColumnName("offer_id")
            .IsRequired();

        builder.Property(od => od.DiscountType)
            .HasColumnName("discount_type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(od => od.DiscountValue)
            .HasColumnName("discount_value")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(od => od.DiscountPeriodMonths)
            .HasColumnName("discount_period_months");

        builder.Property(od => od.ValidFrom)
            .HasColumnName("valid_from");

        builder.Property(od => od.ValidTo)
            .HasColumnName("valid_to");

        builder.Property(od => od.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(od => od.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.HasIndex(od => od.OfferId).HasDatabaseName("ix_offer_discounts_offer_id");
    }
}
