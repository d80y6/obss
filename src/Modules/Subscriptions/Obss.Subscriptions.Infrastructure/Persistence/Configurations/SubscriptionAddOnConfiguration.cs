using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Subscriptions.Domain.Entities;

namespace Obss.Subscriptions.Infrastructure.Persistence.Configurations;

public sealed class SubscriptionAddOnConfiguration : IEntityTypeConfiguration<SubscriptionAddOn>
{
    public void Configure(EntityTypeBuilder<SubscriptionAddOn> builder)
    {
        builder.ToTable("subscription_add_ons");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .ValueGeneratedNever();

        builder.Property(a => a.SubscriptionId)
            .HasColumnName("subscription_id")
            .IsRequired();

        builder.Property(a => a.OfferId)
            .HasColumnName("offer_id")
            .IsRequired();

        builder.Property(a => a.OfferName)
            .HasColumnName("offer_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(a => a.Price)
            .HasColumnName("price")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(a => a.Quantity)
            .HasColumnName("quantity")
            .IsRequired();

        builder.Property(a => a.StartDate)
            .HasColumnName("start_date")
            .IsRequired();

        builder.Property(a => a.EndDate)
            .HasColumnName("end_date");

        builder.Property(a => a.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.HasIndex(a => a.SubscriptionId)
            .HasDatabaseName("ix_subscription_add_ons_subscription_id");
    }
}
