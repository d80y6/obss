using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.NetworkInventory.Domain.Entities;
using Obss.SharedKernel.Domain.ValueObjects;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.NetworkInventory.Infrastructure.Persistence.Configurations;

public sealed class ConnectivityLinkConfiguration : IEntityTypeConfiguration<ConnectivityLink>
{
    public void Configure(EntityTypeBuilder<ConnectivityLink> builder)
    {
        builder.ToTable("connectivity_links");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id)
            .ValueGeneratedNever();

        builder.Property(l => l.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion<TenantIdValueConverter>()
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(l => l.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(l => l.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(l => l.SourceElementId)
            .HasColumnName("source_element_id")
            .IsRequired();

        builder.Property(l => l.SourceInterfaceId)
            .HasColumnName("source_interface_id")
            .IsRequired();

        builder.Property(l => l.TargetElementId)
            .HasColumnName("target_element_id")
            .IsRequired();

        builder.Property(l => l.TargetInterfaceId)
            .HasColumnName("target_interface_id")
            .IsRequired();

        builder.Property(l => l.LinkType)
            .HasColumnName("link_type")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(l => l.Bandwidth)
            .HasColumnName("bandwidth")
            .IsRequired();

        builder.Property(l => l.Status)
            .HasColumnName("status")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(l => l.Protocol)
            .HasColumnName("protocol")
            .HasMaxLength(50);

        builder.Property(l => l.LatencyMs)
            .HasColumnName("latency_ms")
            .IsRequired();

        builder.Property(l => l.MTU)
            .HasColumnName("mtu")
            .IsRequired();

        builder.Property(l => l.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(l => l.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasOne<NetworkElement>()
            .WithMany()
            .HasForeignKey(l => l.SourceElementId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<NetworkElement>()
            .WithMany()
            .HasForeignKey(l => l.TargetElementId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(l => l.SourceElementId)
            .HasDatabaseName("ix_connectivity_links_source_element_id");

        builder.HasIndex(l => l.TargetElementId)
            .HasDatabaseName("ix_connectivity_links_target_element_id");

        builder.HasIndex(l => l.Status)
            .HasDatabaseName("ix_connectivity_links_status");
    }
}
