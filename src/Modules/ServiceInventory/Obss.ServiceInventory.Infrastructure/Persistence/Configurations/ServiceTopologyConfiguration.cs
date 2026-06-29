using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.ServiceInventory.Domain.Entities;

namespace Obss.ServiceInventory.Infrastructure.Persistence.Configurations;

public sealed class ServiceTopologyConfiguration : IEntityTypeConfiguration<ServiceTopology>
{
    public void Configure(EntityTypeBuilder<ServiceTopology> builder)
    {
        builder.ToTable("service_topologies");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .ValueGeneratedNever();

        builder.Property(t => t.ServiceId)
            .HasColumnName("service_id")
            .IsRequired();

        builder.Property(t => t.TopologyType)
            .HasColumnName("topology_type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasMany(t => t.Links)
            .WithOne()
            .HasForeignKey(l => l.ServiceTopologyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(t => t.ServiceId)
            .HasDatabaseName("ix_service_topologies_service_id")
            .IsUnique();

        builder.Navigation(t => t.Links)
            .AutoInclude();
    }
}
