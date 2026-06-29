using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Audit.Domain.Entities;

namespace Obss.Audit.Infrastructure.Persistence.Configurations;

public sealed class AuditPolicyConfiguration : IEntityTypeConfiguration<AuditPolicy>
{
    public void Configure(EntityTypeBuilder<AuditPolicy> builder)
    {
        builder.ToTable("audit_policies");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        builder.Property(p => p.TenantId)
            .HasColumnName("tenant_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.EntityType)
            .HasColumnName("entity_type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.RetentionDays)
            .HasColumnName("retention_days")
            .IsRequired();

        builder.Property(p => p.AlertOnFailure)
            .HasColumnName("alert_on_failure")
            .IsRequired();

        builder.Property(p => p.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.HasIndex(p => p.TenantId)
            .HasDatabaseName("ix_audit_policies_tenant_id");

        builder.HasIndex(p => p.EntityType)
            .HasDatabaseName("ix_audit_policies_entity_type");

        builder.HasIndex("TenantId", "EntityType")
            .HasDatabaseName("ix_audit_policies_tenant_id_entity_type");
    }
}
