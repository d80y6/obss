using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Notifications.Domain.Entities;

namespace Obss.Notifications.Infrastructure.Persistence.Configurations;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Id)
            .ValueGeneratedNever();

        builder.Property(n => n.TenantId)
            .HasColumnName("tenant_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(n => n.NotificationType)
            .HasColumnName("notification_type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(n => n.Channel)
            .HasColumnName("channel")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(n => n.Recipient)
            .HasColumnName("recipient")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(n => n.Subject)
            .HasColumnName("subject")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(n => n.Body)
            .HasColumnName("body")
            .IsRequired();

        builder.Property(n => n.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(n => n.Priority)
            .HasColumnName("priority")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(n => n.SentAt)
            .HasColumnName("sent_at");

        builder.Property(n => n.DeliveredAt)
            .HasColumnName("delivered_at");

        builder.Property(n => n.FailedAt)
            .HasColumnName("failed_at");

        builder.Property(n => n.ErrorMessage)
            .HasColumnName("error_message");

        builder.Property(n => n.ReferenceType)
            .HasColumnName("reference_type")
            .HasMaxLength(100);

        builder.Property(n => n.ReferenceId)
            .HasColumnName("reference_id");

        builder.Property(n => n.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(n => n.TenantId)
            .HasDatabaseName("ix_notifications_tenant_id");

        builder.HasIndex(n => n.Status)
            .HasDatabaseName("ix_notifications_status");

        builder.HasIndex(n => n.ReferenceType)
            .HasDatabaseName("ix_notifications_reference_type");

        builder.HasIndex(n => new { n.Status, n.CreatedAt })
            .HasDatabaseName("ix_notifications_status_created_at");
    }
}
