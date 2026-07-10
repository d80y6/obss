using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Orders.Domain.Entities;

namespace Obss.Orders.Infrastructure.Persistence.Configurations;

public sealed class ProductOrderPaymentConfiguration : IEntityTypeConfiguration<ProductOrderPayment>
{
    public void Configure(EntityTypeBuilder<ProductOrderPayment> builder)
    {
        builder.ToTable("product_order_payments");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        builder.Property(p => p.OrderId)
            .HasColumnName("order_id")
            .IsRequired();

        builder.Property(p => p.Amount)
            .HasColumnName("amount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(p => p.PaymentMethod)
            .HasColumnName("payment_method")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.PaymentReference)
            .HasColumnName("payment_reference")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.PaidAt)
            .HasColumnName("paid_at")
            .IsRequired();

        builder.Property(p => p.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
    }
}
