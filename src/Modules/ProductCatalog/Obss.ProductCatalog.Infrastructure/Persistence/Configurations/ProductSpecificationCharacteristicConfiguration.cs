using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.ProductCatalog.Domain.Domain.Entities;

namespace Obss.ProductCatalog.Infrastructure.Persistence.Configurations;

public sealed class ProductSpecificationCharacteristicConfiguration : IEntityTypeConfiguration<ProductSpecificationCharacteristic>
{
    public void Configure(EntityTypeBuilder<ProductSpecificationCharacteristic> builder)
    {
        builder.ToTable("product_specification_characteristics");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.ProductSpecificationId).HasColumnName("product_specification_id").IsRequired();
        builder.Property(c => c.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(c => c.Description).HasColumnName("description").HasMaxLength(2000);
        builder.Property(c => c.ValueType).HasColumnName("value_type").HasMaxLength(50).IsRequired();
        builder.Property(c => c.Configurable).HasColumnName("configurable").IsRequired();
        builder.Property(c => c.MinValue).HasColumnName("min_value").HasColumnType("decimal(18,4)");
        builder.Property(c => c.MaxValue).HasColumnName("max_value").HasColumnType("decimal(18,4)");
        builder.Property(c => c.Regex).HasColumnName("regex").HasMaxLength(500);
        builder.Property(c => c.SortOrder).HasColumnName("sort_order").IsRequired();
        builder.Property(c => c.MaxCardinality).HasColumnName("max_cardinality");
        builder.Property(c => c.IsRequired).HasColumnName("is_required").IsRequired();

        builder.HasMany(c => c.Values)
            .WithOne()
            .HasForeignKey(v => v.CharacteristicId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.ProductSpecificationId).HasDatabaseName("ix_spec_characteristics_product_specification_id");
    }
}
