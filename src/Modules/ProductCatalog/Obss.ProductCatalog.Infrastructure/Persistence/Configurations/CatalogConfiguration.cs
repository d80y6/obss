using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.ProductCatalog.Domain.Domain.Entities;

namespace Obss.ProductCatalog.Infrastructure.Persistence.Configurations;

public sealed class CatalogConfiguration : IEntityTypeConfiguration<Catalog>
{
    public void Configure(EntityTypeBuilder<Catalog> builder)
    {
        builder.ToTable("catalogs");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.TenantId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Description)
            .HasMaxLength(2000);

        builder.Property(c => c.CatalogType)
            .HasMaxLength(100);

        builder.Property(c => c.LifecycleStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(c => c.Version)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(c => c.ValidFrom);

        builder.Property(c => c.ValidTo);

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .IsRequired();

        builder.Ignore(c => c.Categories);

        builder.HasIndex(c => c.TenantId);
        builder.HasIndex(c => c.Name);
        builder.HasIndex(c => c.CatalogType);
        builder.HasIndex(c => c.LifecycleStatus);
        builder.HasIndex(c => new { c.TenantId, c.Name }).IsUnique();
    }
}
