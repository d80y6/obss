using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.NetworkInventory.Domain.Entities;
using Obss.SharedKernel.Domain.ValueObjects;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.NetworkInventory.Infrastructure.Persistence.Configurations;

public sealed class NetworkElementConfiguration : IEntityTypeConfiguration<NetworkElement>
{
    public void Configure(EntityTypeBuilder<NetworkElement> builder)
    {
        builder.ToTable("network_elements");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedNever();

        builder.Property(e => e.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion<TenantIdValueConverter>()
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Hostname)
            .HasColumnName("hostname")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(e => e.IPAddress)
            .HasColumnName("ip_address")
            .HasMaxLength(45)
            .IsRequired();

        builder.Property(e => e.ElementType)
            .HasColumnName("element_type")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(e => e.Vendor)
            .HasColumnName("vendor")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Model)
            .HasColumnName("model")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.SoftwareVersion)
            .HasColumnName("software_version")
            .HasMaxLength(100);

        builder.Property(e => e.SerialNumber)
            .HasColumnName("serial_number")
            .HasMaxLength(100);

        builder.Property(e => e.Location)
            .HasColumnName("location")
            .HasMaxLength(500);

        builder.Property(e => e.Status)
            .HasColumnName("status")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(e => e.ManagementIP)
            .HasColumnName("management_ip")
            .HasMaxLength(45);

        builder.Property(e => e.SNMPCommunity)
            .HasColumnName("snmp_community")
            .HasMaxLength(200);

        builder.Property(e => e.IsManaged)
            .HasColumnName("is_managed")
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasMany(e => e.Interfaces)
            .WithOne()
            .HasForeignKey(i => i.NetworkElementId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.IpAddresses)
            .WithOne()
            .HasForeignKey(ip => ip.NetworkElementId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.Hostname)
            .HasDatabaseName("ix_network_elements_hostname");

        builder.HasIndex(e => e.ElementType)
            .HasDatabaseName("ix_network_elements_element_type");

        builder.HasIndex(e => e.Status)
            .HasDatabaseName("ix_network_elements_status");

        builder.HasIndex(e => new { e.TenantId, e.Hostname })
            .HasDatabaseName("ix_network_elements_tenant_id_hostname")
            .IsUnique();

        builder.Navigation(e => e.Interfaces)
            .AutoInclude();

        builder.Navigation(e => e.IpAddresses)
            .AutoInclude();
    }
}
