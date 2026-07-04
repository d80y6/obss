using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.ProductCatalog.Domain.Domain.ValueObjects;
using ProductSpecification = Obss.ProductCatalog.Domain.Domain.Entities.ProductSpecification;

namespace Obss.ProductCatalog.Infrastructure.Persistence.Configurations;

public sealed class ProductSpecificationConfiguration : IEntityTypeConfiguration<ProductSpecification>
{
    public void Configure(EntityTypeBuilder<ProductSpecification> builder)
    {
        builder.ToTable("catalog_product_specifications");

        builder.HasKey(ps => ps.Id);

        builder.Property(ps => ps.Id).ValueGeneratedNever();

        builder.Property(ps => ps.TenantId)
            .HasColumnName("tenant_id").HasMaxLength(100).IsRequired();

        builder.Property(ps => ps.Name)
            .HasColumnName("name").HasMaxLength(200).IsRequired();

        builder.Property(ps => ps.Description)
            .HasColumnName("description").HasMaxLength(2000);

        builder.Property(ps => ps.Brand)
            .HasColumnName("brand").HasMaxLength(200);

        builder.Property(ps => ps.Version)
            .HasColumnName("version").HasMaxLength(100);

        builder.Property(ps => ps.ProductNumber)
            .HasColumnName("product_number").HasMaxLength(100);

        builder.Property(ps => ps.LifecycleStatus)
            .HasColumnName("lifecycle_status")
            .HasConversion<string>().HasMaxLength(50).IsRequired();

        builder.Property(ps => ps.ValidFrom).HasColumnName("valid_from");
        builder.Property(ps => ps.ValidTo).HasColumnName("valid_to");
        builder.Property(ps => ps.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(ps => ps.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.HasMany(ps => ps.Characteristics)
            .WithOne()
            .HasForeignKey(c => c.ProductSpecificationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(ps => ps.Relationships)
            .WithOne()
            .HasForeignKey(r => r.ProductSpecificationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(ps => ps.TenantId).HasDatabaseName("ix_product_specifications_tenant_id");
        builder.HasIndex(ps => ps.Name).HasDatabaseName("ix_product_specifications_name");
        builder.HasIndex(ps => ps.LifecycleStatus).HasDatabaseName("ix_product_specifications_lifecycle_status");
        builder.HasIndex(ps => new { ps.TenantId, ps.ProductNumber })
            .HasDatabaseName("ix_product_specifications_tenant_id_product_number")
            .IsUnique()
            .HasFilter("\"product_number\" IS NOT NULL");
    }
}
