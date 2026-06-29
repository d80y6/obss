using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.ProductCatalog.Domain.Domain.Entities;

namespace Obss.ProductCatalog.Infrastructure.Persistence.Configurations;

public sealed class OptionValueConfiguration : IEntityTypeConfiguration<OptionValue>
{
    public void Configure(EntityTypeBuilder<OptionValue> builder)
    {
        builder.ToTable("option_values");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Id)
            .ValueGeneratedNever();

        builder.Property(v => v.ProductOptionId)
            .HasColumnName("product_option_id")
            .IsRequired();

        builder.Property(v => v.Value)
            .HasColumnName("value")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(v => v.DisplayName)
            .HasColumnName("display_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(v => v.PriceAdjustment)
            .HasColumnName("price_adjustment")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(v => v.IsDefault)
            .HasColumnName("is_default")
            .IsRequired();

        builder.Property(v => v.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.HasIndex(v => v.ProductOptionId).HasDatabaseName("ix_option_values_product_option_id");
    }
}
