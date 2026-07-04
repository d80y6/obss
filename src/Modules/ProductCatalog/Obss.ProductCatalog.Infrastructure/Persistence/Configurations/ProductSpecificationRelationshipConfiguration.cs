using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.ProductCatalog.Domain.Domain.Entities;

namespace Obss.ProductCatalog.Infrastructure.Persistence.Configurations;

public sealed class ProductSpecificationRelationshipConfiguration : IEntityTypeConfiguration<ProductSpecificationRelationship>
{
    public void Configure(EntityTypeBuilder<ProductSpecificationRelationship> builder)
    {
        builder.ToTable("product_specification_relationships");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();

        builder.Property(r => r.ProductSpecificationId).HasColumnName("product_specification_id").IsRequired();
        builder.Property(r => r.TargetSpecificationId).HasColumnName("target_specification_id").IsRequired();

        builder.Property(r => r.RelationshipType)
            .HasColumnName("relationship_type")
            .HasConversion<string>().HasMaxLength(50).IsRequired();

        builder.Property(r => r.Role).HasColumnName("role").HasMaxLength(100);
        builder.Property(r => r.ValidFrom).HasColumnName("valid_from");
        builder.Property(r => r.ValidTo).HasColumnName("valid_to");

        builder.HasIndex(r => r.ProductSpecificationId).HasDatabaseName("ix_spec_relationships_product_specification_id");
        builder.HasIndex(r => r.TargetSpecificationId).HasDatabaseName("ix_spec_relationships_target_specification_id");
    }
}
