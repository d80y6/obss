using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.NetworkInventory.Domain.Entities;

namespace Obss.NetworkInventory.Infrastructure.Persistence.Configurations;

public sealed class PONPortConfiguration : IEntityTypeConfiguration<PONPort>
{
    public void Configure(EntityTypeBuilder<PONPort> builder)
    {
        builder.ToTable("pon_ports");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        builder.Property(p => p.OLTId)
            .HasColumnName("olt_id")
            .IsRequired();

        builder.Property(p => p.PortNumber)
            .HasColumnName("port_number")
            .IsRequired();

        builder.Property(p => p.PortType)
            .HasColumnName("port_type")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(p => p.Status)
            .HasColumnName("status")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(p => p.MaxONT)
            .HasColumnName("max_ont")
            .IsRequired();

        builder.Property(p => p.ConnectedONTCount)
            .HasColumnName("connected_ont_count")
            .IsRequired();

        builder.Property(p => p.MaxDistance)
            .HasColumnName("max_distance")
            .IsRequired();

        builder.HasIndex(p => new { p.OLTId, p.PortNumber })
            .HasDatabaseName("ix_pon_ports_olt_id_port_number")
            .IsUnique();
    }
}
