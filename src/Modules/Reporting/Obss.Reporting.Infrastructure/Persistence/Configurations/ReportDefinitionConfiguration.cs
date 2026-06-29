using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Reporting.Domain.Entities;

namespace Obss.Reporting.Infrastructure.Persistence.Configurations;

public sealed class ReportDefinitionConfiguration : IEntityTypeConfiguration<ReportDefinition>
{
    public void Configure(EntityTypeBuilder<ReportDefinition> builder)
    {
        builder.ToTable("report_definitions");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .ValueGeneratedNever();

        builder.Property(r => r.TenantId)
            .HasColumnName("tenant_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(r => r.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(r => r.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(r => r.ReportType)
            .HasColumnName("report_type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(r => r.DataSource)
            .HasColumnName("data_source")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(r => r.Query)
            .HasColumnName("query")
            .IsRequired();

        builder.Property(r => r.OutputFormat)
            .HasColumnName("output_format")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(r => r.Schedule)
            .HasColumnName("schedule")
            .HasMaxLength(100);

        builder.Property(r => r.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(r => r.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(r => r.TenantId)
            .HasDatabaseName("ix_report_definitions_tenant_id");
    }
}
