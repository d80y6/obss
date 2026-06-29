using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.ServiceInventory.Domain.Entities;

namespace Obss.ServiceInventory.Infrastructure.Persistence.Configurations;

public sealed class ServiceResourceConfiguration : IEntityTypeConfiguration<ServiceResource>
{
    public void Configure(EntityTypeBuilder<ServiceResource> builder)
    {
        builder.ToTable("service_resources");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .ValueGeneratedNever();

        builder.Property(r => r.ServiceId)
            .HasColumnName("service_id")
            .IsRequired();

        builder.Property(r => r.ResourceType)
            .HasColumnName("resource_type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(r => r.ResourceIdentifier)
            .HasColumnName("resource_identifier")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(r => r.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(r => r.AllocatedAt)
            .HasColumnName("allocated_at")
            .IsRequired();

        builder.Property(r => r.ReleasedAt)
            .HasColumnName("released_at");

        builder.HasIndex(r => r.ServiceId)
            .HasDatabaseName("ix_service_resources_service_id");

        builder.HasIndex(r => r.ResourceIdentifier)
            .HasDatabaseName("ix_service_resources_resource_identifier");
    }
}
