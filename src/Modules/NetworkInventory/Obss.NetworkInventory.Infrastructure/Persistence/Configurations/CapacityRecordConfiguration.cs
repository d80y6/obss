using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.NetworkInventory.Domain.Entities;

namespace Obss.NetworkInventory.Infrastructure.Persistence.Configurations;

public sealed class CapacityRecordConfiguration : IEntityTypeConfiguration<CapacityRecord>
{
    public void Configure(EntityTypeBuilder<CapacityRecord> builder)
    {
        builder.ToTable("capacity_records");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .ValueGeneratedNever();

        builder.Property(r => r.ElementId)
            .HasColumnName("element_id")
            .IsRequired();

        builder.Property(r => r.InterfaceId)
            .HasColumnName("interface_id");

        builder.Property(r => r.CapacityType)
            .HasColumnName("capacity_type")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(r => r.TotalCapacity)
            .HasColumnName("total_capacity")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(r => r.UsedCapacity)
            .HasColumnName("used_capacity")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(r => r.AvailableCapacity)
            .HasColumnName("available_capacity")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(r => r.UtilizationPercent)
            .HasColumnName("utilization_percent")
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(r => r.MeasuredAt)
            .HasColumnName("measured_at")
            .IsRequired();

        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasOne<NetworkElement>()
            .WithMany()
            .HasForeignKey(r => r.ElementId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.ElementId)
            .HasDatabaseName("ix_capacity_records_element_id");

        builder.HasIndex(r => r.UtilizationPercent)
            .HasDatabaseName("ix_capacity_records_utilization_percent");
    }
}
