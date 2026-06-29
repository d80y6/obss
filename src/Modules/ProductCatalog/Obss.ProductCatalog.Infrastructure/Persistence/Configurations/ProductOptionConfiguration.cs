using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.ProductCatalog.Domain.Domain.Entities;
using Obss.ProductCatalog.Domain.Domain.ValueObjects;

namespace Obss.ProductCatalog.Infrastructure.Persistence.Configurations;

public sealed class ProductOptionConfiguration : IEntityTypeConfiguration<ProductOption>
{
    public void Configure(EntityTypeBuilder<ProductOption> builder)
    {
        builder.ToTable("product_options");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .ValueGeneratedNever();

        builder.Property(o => o.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(o => o.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(o => o.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(o => o.OptionType)
            .HasColumnName("option_type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(o => o.IsRequired)
            .HasColumnName("is_required")
            .IsRequired();

        builder.Property(o => o.IsMultiSelect)
            .HasColumnName("is_multi_select")
            .IsRequired();

        builder.Property(o => o.SortOrder)
            .HasColumnName("sort_order")
            .IsRequired();

        builder.Property(o => o.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.HasMany(o => o.Values)
            .WithOne()
            .HasForeignKey(v => v.ProductOptionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(o => o.ProductId).HasDatabaseName("ix_product_options_product_id");
        builder.HasIndex(o => o.OptionType).HasDatabaseName("ix_product_options_option_type");
    }
}
