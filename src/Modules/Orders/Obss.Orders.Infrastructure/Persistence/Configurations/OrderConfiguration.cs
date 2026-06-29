using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Orders.Domain.Entities;

namespace Obss.Orders.Infrastructure.Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .ValueGeneratedNever();

        builder.Property(o => o.TenantId)
            .HasColumnName("tenant_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(o => o.OrderNumber)
            .HasColumnName("order_number")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(o => o.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.Property(o => o.CustomerName)
            .HasColumnName("customer_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(o => o.OrderDate)
            .HasColumnName("order_date")
            .IsRequired();

        builder.Property(o => o.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(o => o.OrderType)
            .HasColumnName("order_type")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(o => o.SubTotal)
            .HasColumnName("sub_total")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(o => o.TaxTotal)
            .HasColumnName("tax_total")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(o => o.DiscountTotal)
            .HasColumnName("discount_total")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(o => o.GrandTotal)
            .HasColumnName("grand_total")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(o => o.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(o => o.Notes)
            .HasColumnName("notes")
            .HasMaxLength(2000);

        builder.OwnsOne(o => o.BillingAddress, ab =>
        {
            ab.Property(a => a.Street).HasColumnName("billing_street").HasMaxLength(200);
            ab.Property(a => a.City).HasColumnName("billing_city").HasMaxLength(100);
            ab.Property(a => a.State).HasColumnName("billing_state").HasMaxLength(100);
            ab.Property(a => a.PostalCode).HasColumnName("billing_postal_code").HasMaxLength(20);
            ab.Property(a => a.Country).HasColumnName("billing_country").HasMaxLength(100);
        });

        builder.OwnsOne(o => o.ShippingAddress, sab =>
        {
            sab.Property(a => a.Street).HasColumnName("shipping_street").HasMaxLength(200);
            sab.Property(a => a.City).HasColumnName("shipping_city").HasMaxLength(100);
            sab.Property(a => a.State).HasColumnName("shipping_state").HasMaxLength(100);
            sab.Property(a => a.PostalCode).HasColumnName("shipping_postal_code").HasMaxLength(20);
            sab.Property(a => a.Country).HasColumnName("shipping_country").HasMaxLength(100);
        });

        builder.Property(o => o.CreatedById)
            .HasColumnName("created_by_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(o => o.ApprovedById)
            .HasColumnName("approved_by_id")
            .HasMaxLength(100);

        builder.Property(o => o.ApprovedAt)
            .HasColumnName("approved_at");

        builder.Property(o => o.CancellationReason)
            .HasColumnName("cancellation_reason")
            .HasMaxLength(1000);

        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.Payments)
            .WithOne()
            .HasForeignKey(p => p.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(o => o.Fulfillment)
            .WithOne()
            .HasForeignKey<OrderFulfillment>(f => f.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(o => o.OrderNumber)
            .HasDatabaseName("ix_orders_order_number")
            .IsUnique();

        builder.HasIndex(o => o.TenantId)
            .HasDatabaseName("ix_orders_tenant_id");

        builder.HasIndex(o => o.CustomerId)
            .HasDatabaseName("ix_orders_customer_id");

        builder.HasIndex(o => o.Status)
            .HasDatabaseName("ix_orders_status");

        builder.HasIndex(o => o.OrderDate)
            .HasDatabaseName("ix_orders_order_date");

        builder.Navigation(o => o.Items)
            .AutoInclude();

        builder.Navigation(o => o.Payments)
            .AutoInclude();

        builder.Navigation(o => o.Fulfillment)
            .AutoInclude();
    }
}
