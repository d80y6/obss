using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Reporting.Domain.Entities;

namespace Obss.Reporting.Infrastructure.Persistence.Configurations;

public sealed class ScheduledReportConfiguration : IEntityTypeConfiguration<ScheduledReport>
{
    public void Configure(EntityTypeBuilder<ScheduledReport> builder)
    {
        builder.ToTable("scheduled_reports");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedNever();

        builder.Property(s => s.TenantId)
            .HasColumnName("tenant_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.ReportDefinitionId)
            .HasColumnName("report_definition_id")
            .IsRequired();

        builder.Property(s => s.CronExpression)
            .HasColumnName("cron_expression")
            .HasMaxLength(100)
            .IsRequired();

        builder.PrimitiveCollection(s => s.Recipients)
            .HasColumnName("recipients");

        builder.Property(s => s.LastRunAt)
            .HasColumnName("last_run_at");

        builder.Property(s => s.NextRunAt)
            .HasColumnName("next_run_at");

        builder.Property(s => s.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.HasIndex(s => s.TenantId)
            .HasDatabaseName("ix_scheduled_reports_tenant_id");

        builder.HasIndex(s => s.NextRunAt)
            .HasDatabaseName("ix_scheduled_reports_next_run_at");
    }
}
