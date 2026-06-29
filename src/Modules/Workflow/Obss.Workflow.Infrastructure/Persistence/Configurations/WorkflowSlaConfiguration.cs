using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Workflow.Domain.Entities;

namespace Obss.Workflow.Infrastructure.Persistence.Configurations;

public sealed class WorkflowSlaConfiguration : IEntityTypeConfiguration<WorkflowSla>
{
    public void Configure(EntityTypeBuilder<WorkflowSla> builder)
    {
        builder.ToTable("workflow_slas");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedNever();

        builder.Property(s => s.TenantId)
            .HasColumnName("tenant_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(s => s.WorkflowDefinitionId)
            .HasColumnName("workflow_definition_id")
            .IsRequired();

        builder.Property(s => s.TargetDurationMinutes)
            .HasColumnName("target_duration_minutes")
            .IsRequired();

        builder.Property(s => s.WarningThresholdPercent)
            .HasColumnName("warning_threshold_percent")
            .HasColumnType("decimal(5,4)")
            .IsRequired();

        builder.Property(s => s.EscalationUserId)
            .HasColumnName("escalation_user_id")
            .HasMaxLength(200);

        builder.Property(s => s.EscalationGroup)
            .HasColumnName("escalation_group")
            .HasMaxLength(200);

        builder.Property(s => s.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(s => new { s.TenantId, s.Name })
            .HasDatabaseName("ix_workflow_slas_tenant_name")
            .IsUnique();

        builder.HasIndex(s => s.WorkflowDefinitionId)
            .HasDatabaseName("ix_workflow_slas_workflow_definition_id");
    }
}
