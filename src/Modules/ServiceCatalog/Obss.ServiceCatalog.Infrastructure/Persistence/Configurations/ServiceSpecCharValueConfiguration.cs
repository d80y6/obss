using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.ServiceCatalog.Domain.Entities;

namespace Obss.ServiceCatalog.Infrastructure.Persistence.Configurations;

public sealed class ServiceSpecCharValueConfiguration : IEntityTypeConfiguration<ServiceSpecCharValue>
{
    public void Configure(EntityTypeBuilder<ServiceSpecCharValue> builder)
    {
        builder.ToTable("service_spec_characteristic_values");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.CharacteristicId)
            .HasColumnName("characteristic_id")
            .IsRequired();

        builder.Property(x => x.Value)
            .HasColumnName("value")
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x => x.UnitOfMeasure)
            .HasColumnName("unit_of_measure")
            .HasMaxLength(50);

        builder.Property(x => x.IsDefault)
            .HasColumnName("is_default")
            .IsRequired();

        builder.Property(x => x.ValueFrom)
            .HasColumnName("value_from")
            .HasMaxLength(50);

        builder.Property(x => x.ValueTo)
            .HasColumnName("value_to")
            .HasMaxLength(50);

        builder.Property(x => x.RangeInterval)
            .HasColumnName("range_interval")
            .HasMaxLength(50);

        builder.Property(x => x.ValidFrom)
            .HasColumnName("valid_from");

        builder.Property(x => x.ValidTo)
            .HasColumnName("valid_to");
    }
}
