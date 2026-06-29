using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Provisioning.Domain.Entities;

namespace Obss.Provisioning.Infrastructure.Persistence.Configurations;

public sealed class ProvisioningTaskConfiguration : IEntityTypeConfiguration<ProvisioningTask>
{
    public void Configure(EntityTypeBuilder<ProvisioningTask> builder)
    {
        builder.ToTable("provisioning_tasks");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .ValueGeneratedNever();

        builder.Property(t => t.ProvisioningJobId)
            .HasColumnName("provisioning_job_id")
            .IsRequired();

        builder.Property(t => t.StepNumber)
            .HasColumnName("step_number")
            .IsRequired();

        builder.Property(t => t.TaskType)
            .HasColumnName("task_type")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(t => t.Status)
            .HasColumnName("status")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(t => t.AssignedTo)
            .HasColumnName("assigned_to")
            .HasMaxLength(200);

        builder.Property(t => t.Configuration)
            .HasColumnName("configuration")
            .HasColumnType("jsonb");

        builder.Property(t => t.StartedAt)
            .HasColumnName("started_at");

        builder.Property(t => t.CompletedAt)
            .HasColumnName("completed_at");

        builder.Property(t => t.Result)
            .HasColumnName("result")
            .HasColumnType("jsonb");

        builder.Property(t => t.ErrorMessage)
            .HasColumnName("error_message")
            .HasMaxLength(2000);

        builder.Property(t => t.RetryCount)
            .HasColumnName("retry_count")
            .IsRequired()
            .HasDefaultValue(0);

        builder.HasIndex(t => t.ProvisioningJobId)
            .HasDatabaseName("ix_provisioning_tasks_provisioning_job_id");
    }
}
