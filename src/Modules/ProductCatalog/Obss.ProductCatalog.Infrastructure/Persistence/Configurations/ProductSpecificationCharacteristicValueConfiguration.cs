using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.ProductCatalog.Domain.Domain.Entities;

namespace Obss.ProductCatalog.Infrastructure.Persistence.Configurations;

public sealed class ProductSpecificationCharacteristicValueConfiguration : IEntityTypeConfiguration<ProductSpecificationCharacteristicValue>
{
    public void Configure(EntityTypeBuilder<ProductSpecificationCharacteristicValue> builder)
    {
        builder.ToTable("product_specification_characteristic_values");

        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).ValueGeneratedNever();

        builder.Property(v => v.CharacteristicId).HasColumnName("characteristic_id").IsRequired();
        builder.Property(v => v.Value).HasColumnName("value").HasMaxLength(1000).IsRequired();
        builder.Property(v => v.UnitOfMeasure).HasColumnName("unit_of_measure").HasMaxLength(50);
        builder.Property(v => v.IsDefault).HasColumnName("is_default").IsRequired();
        builder.Property(v => v.ValueFrom).HasColumnName("value_from").HasMaxLength(50);
        builder.Property(v => v.ValueTo).HasColumnName("value_to").HasMaxLength(50);
        builder.Property(v => v.RangeInterval).HasColumnName("range_interval").HasMaxLength(50);
        builder.Property(v => v.ValidFrom).HasColumnName("valid_from");
        builder.Property(v => v.ValidTo).HasColumnName("valid_to");

        builder.HasIndex(v => v.CharacteristicId).HasDatabaseName("ix_spec_char_values_characteristic_id");
    }
}
