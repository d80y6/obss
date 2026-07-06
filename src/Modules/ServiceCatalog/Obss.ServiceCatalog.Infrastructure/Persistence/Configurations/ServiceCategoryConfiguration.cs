using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.ServiceCatalog.Domain.Entities;

namespace Obss.ServiceCatalog.Infrastructure.Persistence.Configurations;

public sealed class ServiceCategoryConfiguration : IEntityTypeConfiguration<ServiceCategory>
{
    public void Configure(EntityTypeBuilder<ServiceCategory> builder)
    {
        builder.ToTable("service_categories");

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

        builder.Property(x => x.ParentCategoryId)
            .HasColumnName("parent_category_id");

        builder.Property(x => x.LifecycleStatus)
            .HasColumnName("lifecycle_status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Version)
            .HasColumnName("version")
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

        builder.Ignore(x => x.IsRoot);

        builder.HasIndex(x => x.TenantId).HasDatabaseName("ix_service_categories_tenant_id");
        builder.HasIndex(x => x.ParentCategoryId).HasDatabaseName("ix_service_categories_parent_category_id");
        builder.HasIndex(x => x.LifecycleStatus).HasDatabaseName("ix_service_categories_lifecycle_status");
        builder.HasIndex(x => new { x.TenantId, x.Name }).HasDatabaseName("ix_service_categories_tenant_id_name");

        builder.HasMany(x => x.Candidates)
            .WithMany(x => x.Categories)
            .UsingEntity(j => j.ToTable("category_candidates"));
    }
}
