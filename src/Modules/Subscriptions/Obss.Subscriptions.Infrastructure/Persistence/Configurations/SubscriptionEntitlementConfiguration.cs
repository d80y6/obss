using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Subscriptions.Domain.Entities;
using Obss.Subscriptions.Domain.ValueObjects;

namespace Obss.Subscriptions.Infrastructure.Persistence.Configurations;

public sealed class SubscriptionEntitlementConfiguration : IEntityTypeConfiguration<SubscriptionEntitlement>
{
    public void Configure(EntityTypeBuilder<SubscriptionEntitlement> builder)
    {
        builder.ToTable("subscription_entitlements");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedNever();

        builder.Property(e => e.SubscriptionId)
            .HasColumnName("subscription_id")
            .IsRequired();

        builder.Property(e => e.EntitlementType)
            .HasColumnName("entitlement_type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Limit)
            .HasColumnName("limit_value")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(e => e.Used)
            .HasColumnName("used")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(e => e.Unit)
            .HasColumnName("unit")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.IsUnlimited)
            .HasColumnName("is_unlimited")
            .IsRequired();

        builder.Property(e => e.IsOverridable)
            .HasColumnName("is_overridable")
            .IsRequired();

        builder.Property(e => e.ValidFrom)
            .HasColumnName("valid_from")
            .IsRequired();

        builder.Property(e => e.ValidTo)
            .HasColumnName("valid_to");

        builder.HasOne(e => e.Subscription)
            .WithMany()
            .HasForeignKey(e => e.SubscriptionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.SubscriptionId)
            .HasDatabaseName("ix_subscription_entitlements_subscription_id");

        builder.HasIndex(e => new { e.SubscriptionId, e.EntitlementType })
            .HasDatabaseName("ix_subscription_entitlements_subscription_id_type")
            .IsUnique();
    }
}
