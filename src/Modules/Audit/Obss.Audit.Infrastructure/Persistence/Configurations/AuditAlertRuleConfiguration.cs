using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Audit.Domain.Entities;

namespace Obss.Audit.Infrastructure.Persistence.Configurations;

public sealed class AuditAlertRuleConfiguration : IEntityTypeConfiguration<AuditAlertRule>
{
    public void Configure(EntityTypeBuilder<AuditAlertRule> builder)
    {
        builder.ToTable("audit_alert_rules");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .ValueGeneratedNever();

        builder.Property(r => r.TenantId)
            .HasColumnName("tenant_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(r => r.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(r => r.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(r => r.AlertType)
            .HasColumnName("alert_type")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(r => r.Severity)
            .HasColumnName("severity")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(r => r.Threshold)
            .HasColumnName("threshold")
            .IsRequired();

        builder.Property(r => r.WindowMinutes)
            .HasColumnName("window_minutes")
            .IsRequired();

        builder.Property(r => r.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(r => r.TenantId)
            .HasDatabaseName("ix_audit_alert_rules_tenant_id");

        builder.HasIndex(r => r.AlertType)
            .HasDatabaseName("ix_audit_alert_rules_alert_type");

        builder.HasIndex(r => r.IsActive)
            .HasDatabaseName("ix_audit_alert_rules_is_active");
    }
}
