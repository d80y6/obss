using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Orders.Domain.Entities;

namespace Obss.Orders.Infrastructure.Persistence.Configurations;

public sealed class ProductOrderItemConfiguration : IEntityTypeConfiguration<ProductOrderItem>
{
    public void Configure(EntityTypeBuilder<ProductOrderItem> builder)
    {
        builder.ToTable("order_items");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .ValueGeneratedNever();

        builder.Property(i => i.OrderId)
            .HasColumnName("order_id")
            .IsRequired();

        builder.Property(i => i.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(i => i.OfferId)
            .HasColumnName("offer_id")
            .IsRequired();

        builder.Property(i => i.ProductName)
            .HasColumnName("product_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(i => i.OfferName)
            .HasColumnName("offer_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(i => i.Quantity)
            .HasColumnName("quantity")
            .IsRequired();

        builder.Property(i => i.UnitPrice)
            .HasColumnName("unit_price")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(i => i.RecurringPrice)
            .HasColumnName("recurring_price")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(i => i.DiscountAmount)
            .HasColumnName("discount_amount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(i => i.TaxAmount)
            .HasColumnName("tax_amount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(i => i.TotalPrice)
            .HasColumnName("total_price")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(i => i.BillingPeriod)
            .HasColumnName("billing_period")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(i => i.StartDate)
            .HasColumnName("start_date")
            .IsRequired();

        builder.Property(i => i.EndDate)
            .HasColumnName("end_date");

        builder.Property(i => i.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(i => i.ServiceType)
            .HasColumnName("service_type")
            .HasMaxLength(50);

        builder.Property(i => i.State)
            .HasColumnName("state")
            .HasConversion<string>()
            .HasMaxLength(30);
    }
}
