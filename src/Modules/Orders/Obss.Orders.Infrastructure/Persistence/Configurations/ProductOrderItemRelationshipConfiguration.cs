using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Orders.Domain.Entities;

namespace Obss.Orders.Infrastructure.Persistence.Configurations;

public sealed class ProductOrderItemRelationshipConfiguration : IEntityTypeConfiguration<ProductOrderItemRelationship>
{
    public void Configure(EntityTypeBuilder<ProductOrderItemRelationship> builder)
    {
        builder.ToTable("product_order_item_relationships");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();
        builder.Property(r => r.ProductOrderItemId).IsRequired();
        builder.Property(r => r.TargetItemId).IsRequired();
        builder.Property(r => r.Type).HasConversion<string>().HasMaxLength(50).IsRequired();
    }
}
