using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Workflow.Domain.Entities;

namespace Obss.Workflow.Infrastructure.Persistence.Configurations;

public sealed class WorkflowInstanceConfiguration : IEntityTypeConfiguration<WorkflowInstance>
{
    public void Configure(EntityTypeBuilder<WorkflowInstance> builder)
    {
        builder.ToTable("workflow_instances");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .ValueGeneratedNever();

        builder.Property(i => i.WorkflowDefinitionId)
            .HasColumnName("workflow_definition_id")
            .IsRequired();

        builder.Property(i => i.WorkflowDefinitionName)
            .HasColumnName("workflow_definition_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(i => i.TriggerEntityType)
            .HasColumnName("trigger_entity_type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(i => i.TriggerEntityId)
            .HasColumnName("trigger_entity_id")
            .IsRequired();

        builder.Property(i => i.Status)
            .HasColumnName("status")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(i => i.StartedAt)
            .HasColumnName("started_at");

        builder.Property(i => i.CompletedAt)
            .HasColumnName("completed_at");

        builder.Property(i => i.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(i => i.SlaDeadline)
            .HasColumnName("sla_deadline");

        builder.Property(i => i.SlaBreachedAt)
            .HasColumnName("sla_breached_at");

        builder.HasMany(i => i.Tasks)
            .WithOne(t => t.WorkflowInstance)
            .HasForeignKey(t => t.WorkflowInstanceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(i => i.Status)
            .HasDatabaseName("ix_workflow_instances_status");

        builder.HasIndex(i => new { i.TriggerEntityType, i.TriggerEntityId })
            .HasDatabaseName("ix_workflow_instances_trigger_entity");

        builder.Navigation(i => i.Tasks)
            .AutoInclude();
    }
}
