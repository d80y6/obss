using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Ticketing.Domain.Entities;

namespace Obss.Ticketing.Infrastructure.Persistence.Configurations;

public sealed class AlarmConfiguration : IEntityTypeConfiguration<Alarm>
{
    public void Configure(EntityTypeBuilder<Alarm> builder)
    {
        builder.ToTable("alarms");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .ValueGeneratedNever();

        builder.Property(a => a.AlarmId)
            .HasColumnName("alarm_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.SourceType)
            .HasColumnName("source_type")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(a => a.SourceName)
            .HasColumnName("source_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(a => a.AlarmType)
            .HasColumnName("alarm_type")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(a => a.Severity)
            .HasColumnName("severity")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(a => a.SpecificProblem)
            .HasColumnName("specific_problem")
            .HasMaxLength(500);

        builder.Property(a => a.SpecificProblemAr)
            .HasColumnName("specific_problem_ar")
            .HasMaxLength(500);

        builder.Property(a => a.AffectedServiceId)
            .HasColumnName("affected_service_id");

        builder.Property(a => a.AffectedCustomerId)
            .HasColumnName("affected_customer_id");

        builder.Property(a => a.RaisedTime)
            .HasColumnName("raised_time")
            .IsRequired();

        builder.Property(a => a.AcknowledgedTime)
            .HasColumnName("acknowledged_time");

        builder.Property(a => a.AcknowledgedBy)
            .HasColumnName("acknowledged_by")
            .HasMaxLength(100);

        builder.Property(a => a.ClearedTime)
            .HasColumnName("cleared_time");

        builder.Property(a => a.IsCleared)
            .HasColumnName("is_cleared")
            .IsRequired();

        builder.Property(a => a.DuplicateCount)
            .HasColumnName("duplicate_count")
            .IsRequired();

        builder.Property(a => a.CorrelationRule)
            .HasColumnName("correlation_rule")
            .HasMaxLength(200);

        builder.Property(a => a.MaintenanceWindowId)
            .HasColumnName("maintenance_window_id")
            .HasMaxLength(50);

        builder.HasIndex(a => a.AlarmId)
            .HasDatabaseName("ix_alarms_alarm_id");

        builder.HasIndex(a => a.SourceType)
            .HasDatabaseName("ix_alarms_source_type");

        builder.HasIndex(a => a.Severity)
            .HasDatabaseName("ix_alarms_severity");

        builder.HasIndex(a => a.RaisedTime)
            .HasDatabaseName("ix_alarms_raised_time");

        builder.HasIndex(a => a.AffectedServiceId)
            .HasDatabaseName("ix_alarms_affected_service_id");

        builder.HasIndex(a => a.AffectedCustomerId)
            .HasDatabaseName("ix_alarms_affected_customer_id");

        builder.HasIndex(a => a.IsCleared)
            .HasDatabaseName("ix_alarms_is_cleared");
    }
}