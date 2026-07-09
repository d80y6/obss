using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Subscriptions.Domain.Entities;

namespace Obss.Subscriptions.Infrastructure.Persistence.Configurations;

public sealed class ProductRelationshipConfiguration : IEntityTypeConfiguration<ProductRelationship>
{
    public void Configure(EntityTypeBuilder<ProductRelationship> builder)
    {
        builder.ToTable("product_relationships");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();
        builder.Property(r => r.RelatedProductId).HasColumnName("related_product_id").IsRequired();
        builder.Property(r => r.Type).HasColumnName("type").HasMaxLength(50).IsRequired().HasConversion<string>();
    }
}
