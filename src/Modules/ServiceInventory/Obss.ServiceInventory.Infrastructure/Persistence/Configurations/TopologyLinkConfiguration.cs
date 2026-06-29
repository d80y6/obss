using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.ServiceInventory.Domain.Entities;

namespace Obss.ServiceInventory.Infrastructure.Persistence.Configurations;

public sealed class TopologyLinkConfiguration : IEntityTypeConfiguration<TopologyLink>
{
    public void Configure(EntityTypeBuilder<TopologyLink> builder)
    {
        builder.ToTable("topology_links");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id)
            .ValueGeneratedNever();

        builder.Property(l => l.ServiceTopologyId)
            .HasColumnName("service_topology_id")
            .IsRequired();

        builder.Property(l => l.SourceServiceId)
            .HasColumnName("source_service_id")
            .IsRequired();

        builder.Property(l => l.TargetServiceId)
            .HasColumnName("target_service_id")
            .IsRequired();

        builder.Property(l => l.LinkType)
            .HasColumnName("link_type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(l => l.Direction)
            .HasColumnName("direction")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(l => l.Attributes)
            .HasColumnName("attributes")
            .HasColumnType("jsonb");

        builder.HasIndex(l => l.ServiceTopologyId)
            .HasDatabaseName("ix_topology_links_service_topology_id");

        builder.HasIndex(l => l.SourceServiceId)
            .HasDatabaseName("ix_topology_links_source_service_id");

        builder.HasIndex(l => l.TargetServiceId)
            .HasDatabaseName("ix_topology_links_target_service_id");
    }
}
