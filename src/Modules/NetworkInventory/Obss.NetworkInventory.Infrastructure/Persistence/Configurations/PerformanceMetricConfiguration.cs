using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.NetworkInventory.Domain.Entities;

namespace Obss.NetworkInventory.Infrastructure.Persistence.Configurations;

public sealed class PerformanceMetricConfiguration : IEntityTypeConfiguration<PerformanceMetric>
{
    public void Configure(EntityTypeBuilder<PerformanceMetric> builder)
    {
        builder.ToTable("performance_metrics");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        builder.Property(p => p.SourceType)
            .HasColumnName("source_type")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.SourceName)
            .HasColumnName("source_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.MetricName)
            .HasColumnName("metric_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.MetricNameAr)
            .HasColumnName("metric_name_ar")
            .HasMaxLength(100);

        builder.Property(p => p.Value)
            .HasColumnName("value")
            .IsRequired();

        builder.Property(p => p.Unit)
            .HasColumnName("unit")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.ServiceId)
            .HasColumnName("service_id");

        builder.Property(p => p.CollectedAt)
            .HasColumnName("collected_at")
            .IsRequired();

        builder.Property(p => p.WarningThreshold)
            .HasColumnName("warning_threshold");

        builder.Property(p => p.CriticalThreshold)
            .HasColumnName("critical_threshold");

        builder.Property(p => p.ThresholdBreached)
            .HasColumnName("threshold_breached")
            .IsRequired();

        builder.HasIndex(p => p.SourceType)
            .HasDatabaseName("ix_performance_metrics_source_type");

        builder.HasIndex(p => p.MetricName)
            .HasDatabaseName("ix_performance_metrics_metric_name");

        builder.HasIndex(p => p.CollectedAt)
            .HasDatabaseName("ix_performance_metrics_collected_at");

        builder.HasIndex(p => p.ServiceId)
            .HasDatabaseName("ix_performance_metrics_service_id");

        builder.HasIndex(p => p.ThresholdBreached)
            .HasDatabaseName("ix_performance_metrics_threshold_breached");
    }
}