using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Notifications.Domain.Entities;

namespace Obss.Notifications.Infrastructure.Persistence.Configurations;

public sealed class NotificationPreferenceConfiguration : IEntityTypeConfiguration<NotificationPreference>
{
    public void Configure(EntityTypeBuilder<NotificationPreference> builder)
    {
        builder.ToTable("notification_preferences");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        builder.Property(p => p.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.Property(p => p.NotificationType)
            .HasColumnName("notification_type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.Channel)
            .HasColumnName("channel")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.IsOptedIn)
            .HasColumnName("is_opted_in")
            .IsRequired();

        builder.Property(p => p.OptInAt)
            .HasColumnName("opt_in_at");

        builder.Property(p => p.OptOutAt)
            .HasColumnName("opt_out_at");

        builder.HasIndex(p => p.CustomerId)
            .HasDatabaseName("ix_notification_preferences_customer_id");

        builder.HasIndex(p => new { p.CustomerId, p.NotificationType, p.Channel })
            .HasDatabaseName("ix_notification_preferences_customer_type_channel")
            .IsUnique();
    }
}
