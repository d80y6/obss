using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Reporting.Domain.Entities;

namespace Obss.Reporting.Infrastructure.Persistence.Configurations;

public sealed class ReportExecutionConfiguration : IEntityTypeConfiguration<ReportExecution>
{
    public void Configure(EntityTypeBuilder<ReportExecution> builder)
    {
        builder.ToTable("report_executions");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedNever();

        builder.Property(e => e.ReportDefinitionId)
            .HasColumnName("report_definition_id")
            .IsRequired();

        builder.Property(e => e.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.StartedAt)
            .HasColumnName("started_at");

        builder.Property(e => e.CompletedAt)
            .HasColumnName("completed_at");

        builder.Property(e => e.FilePath)
            .HasColumnName("file_path")
            .HasMaxLength(500);

        builder.Property(e => e.FileSize)
            .HasColumnName("file_size");

        builder.Property(e => e.ErrorMessage)
            .HasColumnName("error_message");

        builder.Property(e => e.ExecutedBy)
            .HasColumnName("executed_by")
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(e => e.ReportDefinitionId)
            .HasDatabaseName("ix_report_executions_report_definition_id");
    }
}
