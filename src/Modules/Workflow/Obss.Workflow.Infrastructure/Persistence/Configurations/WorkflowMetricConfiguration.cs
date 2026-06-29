using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Workflow.Domain.Entities;

namespace Obss.Workflow.Infrastructure.Persistence.Configurations;

public sealed class WorkflowMetricConfiguration : IEntityTypeConfiguration<WorkflowMetric>
{
    public void Configure(EntityTypeBuilder<WorkflowMetric> builder)
    {
        builder.ToTable("workflow_metrics");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .ValueGeneratedNever();

        builder.Property(m => m.MetricType)
            .HasColumnName("metric_type")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(m => m.WorkflowDefinitionId)
            .HasColumnName("workflow_definition_id")
            .IsRequired();

        builder.Property(m => m.TimeBucket)
            .HasColumnName("time_bucket")
            .IsRequired();

        builder.Property(m => m.Count)
            .HasColumnName("count")
            .IsRequired();

        builder.Property(m => m.Value)
            .HasColumnName("value")
            .HasColumnType("decimal(18,6)")
            .IsRequired();

        builder.Property(m => m.RecordedAt)
            .HasColumnName("recorded_at")
            .IsRequired();

        builder.HasIndex(m => new { m.WorkflowDefinitionId, m.MetricType, m.TimeBucket })
            .HasDatabaseName("ix_workflow_metrics_definition_type_bucket");
    }
}
