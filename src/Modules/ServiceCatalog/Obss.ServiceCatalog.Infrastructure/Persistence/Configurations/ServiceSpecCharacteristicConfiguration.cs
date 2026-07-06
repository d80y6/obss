using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.ServiceCatalog.Domain.Entities;

namespace Obss.ServiceCatalog.Infrastructure.Persistence.Configurations;

public sealed class ServiceSpecCharacteristicConfiguration : IEntityTypeConfiguration<ServiceSpecCharacteristic>
{
    public void Configure(EntityTypeBuilder<ServiceSpecCharacteristic> builder)
    {
        builder.ToTable("service_spec_characteristics");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.ServiceSpecificationId)
            .HasColumnName("service_specification_id")
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(2000);

        builder.Property(x => x.ValueType)
            .HasColumnName("value_type")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Configurable)
            .HasColumnName("configurable")
            .IsRequired();

        builder.Property(x => x.MinValue)
            .HasColumnName("min_value")
            .HasColumnType("decimal(18,4)");

        builder.Property(x => x.MaxValue)
            .HasColumnName("max_value")
            .HasColumnType("decimal(18,4)");

        builder.Property(x => x.Regex)
            .HasColumnName("regex")
            .HasMaxLength(500);

        builder.Property(x => x.SortOrder)
            .HasColumnName("sort_order")
            .IsRequired();

        builder.Property(x => x.MaxCardinality)
            .HasColumnName("max_cardinality");

        builder.Property(x => x.IsRequired)
            .HasColumnName("is_required")
            .IsRequired();

        builder.HasMany(x => x.Values)
            .WithOne()
            .HasForeignKey(v => v.CharacteristicId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
