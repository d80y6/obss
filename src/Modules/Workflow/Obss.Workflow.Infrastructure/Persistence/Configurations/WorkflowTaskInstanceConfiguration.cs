using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Workflow.Domain.Entities;

namespace Obss.Workflow.Infrastructure.Persistence.Configurations;

public sealed class WorkflowTaskInstanceConfiguration : IEntityTypeConfiguration<WorkflowTaskInstance>
{
    public void Configure(EntityTypeBuilder<WorkflowTaskInstance> builder)
    {
        builder.ToTable("workflow_task_instances");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .ValueGeneratedNever();

        builder.Property(t => t.WorkflowInstanceId)
            .HasColumnName("workflow_instance_id")
            .IsRequired();

        builder.Property(t => t.WorkflowStepId)
            .HasColumnName("workflow_step_id")
            .IsRequired();

        builder.Property(t => t.StepName)
            .HasColumnName("step_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Status)
            .HasColumnName("status")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(t => t.AssignedTo)
            .HasColumnName("assigned_to")
            .HasMaxLength(200);

        builder.Property(t => t.StartedAt)
            .HasColumnName("started_at");

        builder.Property(t => t.CompletedAt)
            .HasColumnName("completed_at");

        builder.Property(t => t.Result)
            .HasColumnName("result");

        builder.Property(t => t.ErrorMessage)
            .HasColumnName("error_message");

        builder.Property(t => t.RetryCount)
            .HasColumnName("retry_count")
            .IsRequired();

        builder.HasIndex(t => t.Status)
            .HasDatabaseName("ix_workflow_task_instances_status");

        builder.HasIndex(t => new { t.WorkflowInstanceId, t.WorkflowStepId })
            .HasDatabaseName("ix_workflow_task_instances_instance_step")
            .IsUnique();
    }
}
