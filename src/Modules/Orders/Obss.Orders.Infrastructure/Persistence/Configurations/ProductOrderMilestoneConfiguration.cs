using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Orders.Domain.Entities;

namespace Obss.Orders.Infrastructure.Persistence.Configurations;

public sealed class ProductOrderMilestoneConfiguration : IEntityTypeConfiguration<ProductOrderMilestone>
{
    public void Configure(EntityTypeBuilder<ProductOrderMilestone> builder)
    {
        builder.ToTable("product_order_milestones");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedNever();
        builder.Property(m => m.Name).HasMaxLength(100).IsRequired();
        builder.Property(m => m.Description).HasMaxLength(500);
        builder.Property(m => m.MilestoneDate).IsRequired();
        builder.Property(m => m.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(m => m.ProductOrderId).IsRequired();
        builder.HasIndex(m => m.ProductOrderId);
    }
}
