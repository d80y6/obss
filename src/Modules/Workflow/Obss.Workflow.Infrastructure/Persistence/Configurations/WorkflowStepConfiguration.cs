using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Workflow.Domain.Entities;

namespace Obss.Workflow.Infrastructure.Persistence.Configurations;

public sealed class WorkflowStepConfiguration : IEntityTypeConfiguration<WorkflowStep>
{
    public void Configure(EntityTypeBuilder<WorkflowStep> builder)
    {
        builder.ToTable("workflow_steps");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedNever();

        builder.Property(s => s.WorkflowDefinitionId)
            .HasColumnName("workflow_definition_id")
            .IsRequired();

        builder.Property(s => s.StepNumber)
            .HasColumnName("step_number")
            .IsRequired();

        builder.Property(s => s.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(s => s.StepType)
            .HasColumnName("step_type")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(s => s.HandlerType)
            .HasColumnName("handler_type")
            .HasMaxLength(500);

        builder.Property(s => s.Configuration)
            .HasColumnName("configuration")
            .HasColumnType("jsonb");

        builder.Property(s => s.Timeout)
            .HasColumnName("timeout")
            .IsRequired();

        builder.Property(s => s.RetryCount)
            .HasColumnName("retry_count")
            .IsRequired();

        builder.Property(s => s.RetryDelaySeconds)
            .HasColumnName("retry_delay_seconds")
            .IsRequired();

        builder.Property(s => s.IsRequired)
            .HasColumnName("is_required")
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(s => new { s.WorkflowDefinitionId, s.StepNumber })
            .HasDatabaseName("ix_workflow_steps_definition_step_number")
            .IsUnique();
    }
}
