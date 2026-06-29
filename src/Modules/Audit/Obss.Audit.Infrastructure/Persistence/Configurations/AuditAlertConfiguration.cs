using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Audit.Domain.Entities;

namespace Obss.Audit.Infrastructure.Persistence.Configurations;

public sealed class AuditAlertConfiguration : IEntityTypeConfiguration<AuditAlert>
{
    public void Configure(EntityTypeBuilder<AuditAlert> builder)
    {
        builder.ToTable("audit_alerts");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedNever();

        builder.Property(e => e.TenantId)
            .HasColumnName("tenant_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Severity)
            .HasColumnName("severity")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(e => e.AlertType)
            .HasColumnName("alert_type")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(e => e.Message)
            .HasColumnName("message")
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(e => e.EntityType)
            .HasColumnName("entity_type")
            .HasMaxLength(100);

        builder.Property(e => e.EntityId)
            .HasColumnName("entity_id")
            .HasMaxLength(200);

        builder.Property(e => e.DetectedAt)
            .HasColumnName("detected_at")
            .IsRequired();

        builder.Property(e => e.IsAcknowledged)
            .HasColumnName("is_acknowledged")
            .IsRequired();

        builder.Property(e => e.AcknowledgedById)
            .HasColumnName("acknowledged_by_id")
            .HasMaxLength(200);

        builder.Property(e => e.AcknowledgedAt)
            .HasColumnName("acknowledged_at");

        builder.Property(e => e.ResolvedAt)
            .HasColumnName("resolved_at");

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(e => e.TenantId)
            .HasDatabaseName("ix_audit_alerts_tenant_id");

        builder.HasIndex(e => e.Severity)
            .HasDatabaseName("ix_audit_alerts_severity");

        builder.HasIndex(e => e.AlertType)
            .HasDatabaseName("ix_audit_alerts_alert_type");

        builder.HasIndex(e => e.IsAcknowledged)
            .HasDatabaseName("ix_audit_alerts_is_acknowledged");

        builder.HasIndex(e => e.DetectedAt)
            .HasDatabaseName("ix_audit_alerts_detected_at");
    }
}
