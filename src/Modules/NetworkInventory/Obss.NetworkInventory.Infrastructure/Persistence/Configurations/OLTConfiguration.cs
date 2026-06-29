using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.NetworkInventory.Domain.Entities;
using Obss.SharedKernel.Domain.ValueObjects;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.NetworkInventory.Infrastructure.Persistence.Configurations;

public sealed class OLTConfiguration : IEntityTypeConfiguration<OLT>
{
    public void Configure(EntityTypeBuilder<OLT> builder)
    {
        builder.ToTable("olts");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .ValueGeneratedNever();

        builder.Property(o => o.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion<TenantIdValueConverter>()
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(o => o.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(o => o.Hostname)
            .HasColumnName("hostname")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(o => o.IPAddress)
            .HasColumnName("ip_address")
            .HasMaxLength(45)
            .IsRequired();

        builder.Property(o => o.Vendor)
            .HasColumnName("vendor")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(o => o.Model)
            .HasColumnName("model")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(o => o.SoftwareVersion)
            .HasColumnName("software_version")
            .HasMaxLength(100);

        builder.Property(o => o.Location)
            .HasColumnName("location")
            .HasMaxLength(500);

        builder.Property(o => o.Status)
            .HasColumnName("status")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(o => o.MaxPONPorts)
            .HasColumnName("max_pon_ports")
            .IsRequired();

        builder.Property(o => o.UsedPONPorts)
            .HasColumnName("used_pon_ports")
            .IsRequired();

        builder.Property(o => o.MaxONTPerPort)
            .HasColumnName("max_ont_per_port")
            .IsRequired();

        builder.Property(o => o.MaxBandwidth)
            .HasColumnName("max_bandwidth")
            .IsRequired();

        builder.Property(o => o.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasMany(o => o.PONPorts)
            .WithOne()
            .HasForeignKey(p => p.OLTId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(o => o.Hostname)
            .HasDatabaseName("ix_olts_hostname");

        builder.Navigation(o => o.PONPorts)
            .AutoInclude();
    }
}
