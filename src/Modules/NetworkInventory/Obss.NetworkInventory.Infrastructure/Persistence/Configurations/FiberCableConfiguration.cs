using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.NetworkInventory.Domain.Entities;

namespace Obss.NetworkInventory.Infrastructure.Persistence.Configurations;

public sealed class FiberCableConfiguration : IEntityTypeConfiguration<FiberCable>
{
    public void Configure(EntityTypeBuilder<FiberCable> builder)
    {
        builder.ToTable("fiber_cables");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .ValueGeneratedNever();

        builder.Property(c => c.FromElementId)
            .HasColumnName("from_element_id")
            .IsRequired();

        builder.Property(c => c.FromInterfaceId)
            .HasColumnName("from_interface_id")
            .IsRequired();

        builder.Property(c => c.ToElementId)
            .HasColumnName("to_element_id")
            .IsRequired();

        builder.Property(c => c.ToInterfaceId)
            .HasColumnName("to_interface_id")
            .IsRequired();

        builder.Property(c => c.Length)
            .HasColumnName("length")
            .IsRequired();

        builder.Property(c => c.FiberCount)
            .HasColumnName("fiber_count")
            .IsRequired();

        builder.Property(c => c.FiberType)
            .HasColumnName("fiber_type")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(c => c.Status)
            .HasColumnName("status")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(c => c.SplicingPoints)
            .HasColumnName("splicing_points");

        builder.Property(c => c.Notes)
            .HasColumnName("notes")
            .HasMaxLength(2000);

        builder.HasOne<NetworkElement>()
            .WithMany()
            .HasForeignKey(c => c.FromElementId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<NetworkElement>()
            .WithMany()
            .HasForeignKey(c => c.ToElementId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.FromElementId)
            .HasDatabaseName("ix_fiber_cables_from_element_id");

        builder.HasIndex(c => c.ToElementId)
            .HasDatabaseName("ix_fiber_cables_to_element_id");
    }
}
