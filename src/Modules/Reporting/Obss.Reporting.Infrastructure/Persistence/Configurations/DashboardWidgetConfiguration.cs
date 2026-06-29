using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Reporting.Domain.Entities;

namespace Obss.Reporting.Infrastructure.Persistence.Configurations;

public sealed class DashboardWidgetConfiguration : IEntityTypeConfiguration<DashboardWidget>
{
    public void Configure(EntityTypeBuilder<DashboardWidget> builder)
    {
        builder.ToTable("dashboard_widgets");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Id)
            .ValueGeneratedNever();

        builder.Property(w => w.TenantId)
            .HasColumnName("tenant_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(w => w.WidgetType)
            .HasColumnName("widget_type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(w => w.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(w => w.Configuration)
            .HasColumnName("configuration")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(w => w.Position)
            .HasColumnName("position")
            .IsRequired();

        builder.Property(w => w.Size)
            .HasColumnName("size")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(w => w.DataSource)
            .HasColumnName("data_source")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(w => w.Query)
            .HasColumnName("query")
            .IsRequired();

        builder.Property(w => w.RefreshInterval)
            .HasColumnName("refresh_interval");

        builder.Property(w => w.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(w => w.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(w => w.TenantId)
            .HasDatabaseName("ix_dashboard_widgets_tenant_id");

        builder.HasIndex(w => new { w.TenantId, w.Position })
            .HasDatabaseName("ix_dashboard_widgets_tenant_id_position");
    }
}
