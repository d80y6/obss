using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.ProductCatalog.Domain.Domain.Entities;
using Obss.ProductCatalog.Domain.Domain.ValueObjects;

namespace Obss.ProductCatalog.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        builder.Property(p => p.TenantId)
            .HasColumnName("tenant_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasColumnName("description")
            .HasMaxLength(2000);

        builder.Property(p => p.CategoryId)
            .HasColumnName("category_id");

        builder.Property(p => p.ProductType)
            .HasColumnName("product_type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(p => p.IsShippable)
            .HasColumnName("is_shippable")
            .IsRequired();

        builder.Property(p => p.Taxable)
            .HasColumnName("taxable")
            .IsRequired();

        builder.Property(p => p.TaxCategory)
            .HasColumnName("tax_category")
            .HasMaxLength(100);

        builder.Property(p => p.LifecycleStatus)
            .HasColumnName("lifecycle_status")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.OwnsMany(p => p.Specifications, sb =>
        {
            sb.WithOwner().HasForeignKey("product_id");
            sb.ToTable("product_specifications");
            sb.Property<int>("Id").ValueGeneratedOnAdd();
            sb.HasKey("Id");
            sb.Property(s => s.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            sb.Property(s => s.Value).HasColumnName("value").HasMaxLength(1000).IsRequired();
            sb.Property(s => s.IsRequired).HasColumnName("is_required").IsRequired();
        });

        builder.HasMany(p => p.ProductOffers)
            .WithOne()
            .HasForeignKey(po => po.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.TenantId).HasDatabaseName("ix_products_tenant_id");
        builder.HasIndex(p => p.CategoryId).HasDatabaseName("ix_products_category_id");
        builder.HasIndex(p => p.ProductType).HasDatabaseName("ix_products_product_type");
        builder.HasIndex(p => p.LifecycleStatus).HasDatabaseName("ix_products_lifecycle_status");
        builder.HasIndex(p => new { p.TenantId, p.Name }).HasDatabaseName("ix_products_tenant_id_name");
    }
}
