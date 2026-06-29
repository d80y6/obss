using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Subscriptions.Domain.Entities;

namespace Obss.Subscriptions.Infrastructure.Persistence.Configurations;

public sealed class SubscriptionServiceConfiguration : IEntityTypeConfiguration<SubscriptionService>
{
    public void Configure(EntityTypeBuilder<SubscriptionService> builder)
    {
        builder.ToTable("subscription_services");

        builder.HasKey(ss => ss.Id);

        builder.Property(ss => ss.Id)
            .ValueGeneratedNever();

        builder.Property(ss => ss.SubscriptionId)
            .HasColumnName("subscription_id")
            .IsRequired();

        builder.Property(ss => ss.ServiceId)
            .HasColumnName("service_id")
            .IsRequired();

        builder.Property(ss => ss.ServiceType)
            .HasColumnName("service_type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(ss => ss.Status)
            .HasColumnName("status")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(ss => ss.ProvisionedAt)
            .HasColumnName("provisioned_at")
            .IsRequired();

        builder.HasIndex(ss => ss.SubscriptionId)
            .HasDatabaseName("ix_subscription_services_subscription_id");

        builder.HasIndex(ss => ss.ServiceId)
            .HasDatabaseName("ix_subscription_services_service_id");
    }
}
