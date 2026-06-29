using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.NetworkInventory.Domain.Entities;

namespace Obss.NetworkInventory.Infrastructure.Persistence.Configurations;

public sealed class NetworkElementIpAddressConfiguration : IEntityTypeConfiguration<NetworkElementIpAddress>
{
    public void Configure(EntityTypeBuilder<NetworkElementIpAddress> builder)
    {
        builder.ToTable("network_element_ip_addresses");

        builder.HasKey(ip => ip.Id);

        builder.Property(ip => ip.Id)
            .ValueGeneratedNever();

        builder.Property(ip => ip.NetworkElementId)
            .HasColumnName("network_element_id")
            .IsRequired();

        builder.Property(ip => ip.NetworkInterfaceId)
            .HasColumnName("network_interface_id");

        builder.Property(ip => ip.IPAddress)
            .HasColumnName("ip_address")
            .HasMaxLength(45)
            .IsRequired();

        builder.Property(ip => ip.SubnetMask)
            .HasColumnName("subnet_mask")
            .HasMaxLength(45)
            .IsRequired();

        builder.Property(ip => ip.Gateway)
            .HasColumnName("gateway")
            .HasMaxLength(45);

        builder.Property(ip => ip.AddressType)
            .HasColumnName("address_type")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(ip => ip.IsAvailable)
            .HasColumnName("is_available")
            .IsRequired();

        builder.Property(ip => ip.IsReserved)
            .HasColumnName("is_reserved")
            .IsRequired();

        builder.Property(ip => ip.AssignedTo)
            .HasColumnName("assigned_to")
            .HasMaxLength(200);

        builder.HasIndex(ip => ip.NetworkElementId)
            .HasDatabaseName("ix_network_element_ip_addresses_network_element_id");

        builder.HasIndex(ip => ip.IPAddress)
            .HasDatabaseName("ix_network_element_ip_addresses_ip_address");

        builder.HasIndex(ip => ip.IsAvailable)
            .HasDatabaseName("ix_network_element_ip_addresses_is_available");
    }
}
