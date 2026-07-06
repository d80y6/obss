using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.ServiceCatalog.Domain.Entities;

namespace Obss.ServiceCatalog.Infrastructure.Persistence.Configurations;

public sealed class ServiceSpecificationConfiguration : IEntityTypeConfiguration<ServiceSpecification>
{
    public void Configure(EntityTypeBuilder<ServiceSpecification> builder)
    {
        builder.ToTable("service_specifications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.TenantId)
            .HasColumnName("tenant_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(2000);

        builder.Property(x => x.Brand)
            .HasColumnName("brand")
            .HasMaxLength(200);

        builder.Property(x => x.Version)
            .HasColumnName("version")
            .HasMaxLength(100);

        builder.Property(x => x.LifecycleStatus)
            .HasColumnName("lifecycle_status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.IsBundle)
            .HasColumnName("is_bundle")
            .IsRequired();

        builder.Property(x => x.ValidFrom)
            .HasColumnName("valid_from");

        builder.Property(x => x.ValidTo)
            .HasColumnName("valid_to");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(x => x.TenantId).HasDatabaseName("ix_service_specifications_tenant_id");
        builder.HasIndex(x => x.LifecycleStatus).HasDatabaseName("ix_service_specifications_lifecycle_status");
        builder.HasIndex(x => x.Brand).HasDatabaseName("ix_service_specifications_brand");

        builder.HasMany(x => x.Characteristics)
            .WithOne()
            .HasForeignKey(c => c.ServiceSpecificationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Relationships)
            .WithOne()
            .HasForeignKey(r => r.ServiceSpecificationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
