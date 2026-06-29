using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Subscriptions.Domain.Entities;

namespace Obss.Subscriptions.Infrastructure.Persistence.Configurations;

public sealed class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("subscriptions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedNever();

        builder.Property(s => s.TenantId)
            .HasColumnName("tenant_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.Property(s => s.CustomerName)
            .HasColumnName("customer_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.OrderId)
            .HasColumnName("order_id")
            .IsRequired();

        builder.Property(s => s.OrderItemId)
            .HasColumnName("order_item_id")
            .IsRequired();

        builder.Property(s => s.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(s => s.OfferId)
            .HasColumnName("offer_id")
            .IsRequired();

        builder.Property(s => s.OfferName)
            .HasColumnName("offer_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(s => s.BillingPeriod)
            .HasColumnName("billing_period")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(s => s.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(s => s.Price)
            .HasColumnName("price")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(s => s.Quantity)
            .HasColumnName("quantity")
            .IsRequired();

        builder.Property(s => s.StartDate)
            .HasColumnName("start_date")
            .IsRequired();

        builder.Property(s => s.EndDate)
            .HasColumnName("end_date");

        builder.Property(s => s.CancelledAt)
            .HasColumnName("cancelled_at");

        builder.Property(s => s.SuspendedAt)
            .HasColumnName("suspended_at");

        builder.Property(s => s.ActivationDate)
            .HasColumnName("activation_date");

        builder.Property(s => s.RenewalDate)
            .HasColumnName("renewal_date");

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(s => s.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasMany(s => s.AddOns)
            .WithOne(a => a.Subscription)
            .HasForeignKey(a => a.SubscriptionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Services)
            .WithOne(ss => ss.Subscription)
            .HasForeignKey(ss => ss.SubscriptionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => s.CustomerId)
            .HasDatabaseName("ix_subscriptions_customer_id");

        builder.HasIndex(s => s.Status)
            .HasDatabaseName("ix_subscriptions_status");

        builder.HasIndex(s => s.TenantId)
            .HasDatabaseName("ix_subscriptions_tenant_id");

        builder.HasIndex(s => s.RenewalDate)
            .HasDatabaseName("ix_subscriptions_renewal_date");

        builder.HasIndex(s => new { s.CustomerId, s.Status })
            .HasDatabaseName("ix_subscriptions_customer_id_status");

        builder.Navigation(s => s.AddOns)
            .AutoInclude();

        builder.Navigation(s => s.Services)
            .AutoInclude();
    }
}
