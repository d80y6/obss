using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.NetworkInventory.Domain.Entities;

namespace Obss.NetworkInventory.Infrastructure.Persistence.Configurations;

public sealed class NetworkInterfaceConfiguration : IEntityTypeConfiguration<NetworkInterface>
{
    public void Configure(EntityTypeBuilder<NetworkInterface> builder)
    {
        builder.ToTable("network_interfaces");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .ValueGeneratedNever();

        builder.Property(i => i.NetworkElementId)
            .HasColumnName("network_element_id")
            .IsRequired();

        builder.Property(i => i.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(i => i.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(i => i.InterfaceType)
            .HasColumnName("interface_type")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(i => i.Speed)
            .HasColumnName("speed")
            .IsRequired();

        builder.Property(i => i.Status)
            .HasColumnName("status")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(i => i.MacAddress)
            .HasColumnName("mac_address")
            .HasMaxLength(17);

        builder.Property(i => i.MTU)
            .HasColumnName("mtu")
            .IsRequired();

        builder.Property(i => i.ConnectedToInterfaceId)
            .HasColumnName("connected_to_interface_id");

        builder.Property(i => i.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(i => i.NetworkElementId)
            .HasDatabaseName("ix_network_interfaces_network_element_id");

        builder.HasIndex(i => i.ConnectedToInterfaceId)
            .HasDatabaseName("ix_network_interfaces_connected_to_interface_id")
            .IsUnique()
            .HasFilter("connected_to_interface_id IS NOT NULL");
    }
}
