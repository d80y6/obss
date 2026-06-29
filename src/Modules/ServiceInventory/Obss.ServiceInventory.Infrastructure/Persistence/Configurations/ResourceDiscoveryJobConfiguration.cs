using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.ServiceInventory.Domain.Entities;

namespace Obss.ServiceInventory.Infrastructure.Persistence.Configurations;

public sealed class ResourceDiscoveryJobConfiguration : IEntityTypeConfiguration<ResourceDiscoveryJob>
{
    public void Configure(EntityTypeBuilder<ResourceDiscoveryJob> builder)
    {
        builder.ToTable("resource_discovery_jobs");

        builder.HasKey(j => j.Id);

        builder.Property(j => j.Id)
            .ValueGeneratedNever();

        builder.Property(j => j.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(j => j.DiscoveryType)
            .HasColumnName("discovery_type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(j => j.Configuration)
            .HasColumnName("configuration")
            .HasColumnType("jsonb");

        builder.Property(j => j.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(j => j.StartedAt)
            .HasColumnName("started_at");

        builder.Property(j => j.CompletedAt)
            .HasColumnName("completed_at");

        builder.Property(j => j.ResourcesFound)
            .HasColumnName("resources_found")
            .IsRequired();

        builder.Property(j => j.ResourcesMatched)
            .HasColumnName("resources_matched")
            .IsRequired();

        builder.Property(j => j.ErrorMessage)
            .HasColumnName("error_message")
            .HasMaxLength(2000);

        builder.Property(j => j.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(j => j.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(j => j.TenantId)
            .HasDatabaseName("ix_resource_discovery_jobs_tenant_id");

        builder.HasIndex(j => j.Status)
            .HasDatabaseName("ix_resource_discovery_jobs_status");
    }
}
