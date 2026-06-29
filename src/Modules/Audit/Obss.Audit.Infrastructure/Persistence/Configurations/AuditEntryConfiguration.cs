using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Audit.Domain.Entities;

namespace Obss.Audit.Infrastructure.Persistence.Configurations;

public sealed class AuditEntryConfiguration : IEntityTypeConfiguration<AuditEntry>
{
    public void Configure(EntityTypeBuilder<AuditEntry> builder)
    {
        builder.ToTable("audit_entries");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedNever();

        builder.Property(e => e.TenantId)
            .HasColumnName("tenant_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.EntityType)
            .HasColumnName("entity_type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.EntityId)
            .HasColumnName("entity_id")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Action)
            .HasColumnName("action")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(e => e.Changes)
            .HasColumnName("changes")
            .HasColumnType("jsonb");

        builder.Property(e => e.PerformedById)
            .HasColumnName("performed_by_id")
            .HasMaxLength(200);

        builder.Property(e => e.PerformedByName)
            .HasColumnName("performed_by_name")
            .HasMaxLength(256);

        builder.Property(e => e.PerformedAt)
            .HasColumnName("performed_at")
            .IsRequired();

        builder.Property(e => e.IpAddress)
            .HasColumnName("ip_address")
            .HasMaxLength(50);

        builder.Property(e => e.UserAgent)
            .HasColumnName("user_agent")
            .HasMaxLength(500);

        builder.Property(e => e.CorrelationId)
            .HasColumnName("correlation_id")
            .HasMaxLength(200);

        builder.Property(e => e.IsSensitive)
            .HasColumnName("is_sensitive")
            .IsRequired();

        builder.HasIndex(e => e.TenantId)
            .HasDatabaseName("ix_audit_entries_tenant_id");

        builder.HasIndex(e => e.EntityType)
            .HasDatabaseName("ix_audit_entries_entity_type");

        builder.HasIndex(e => new { e.EntityType, e.EntityId })
            .HasDatabaseName("ix_audit_entries_entity_type_entity_id");

        builder.HasIndex(e => e.PerformedAt)
            .HasDatabaseName("ix_audit_entries_performed_at");

        builder.HasIndex(e => e.Action)
            .HasDatabaseName("ix_audit_entries_action");

        builder.HasIndex(e => e.PerformedById)
            .HasDatabaseName("ix_audit_entries_performed_by_id");
    }
}
