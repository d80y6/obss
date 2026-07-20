using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.NetworkInventory.Domain.Entities;

namespace Obss.NetworkInventory.Infrastructure.Persistence.Configurations;

public sealed class MaintenanceWindowConfiguration : IEntityTypeConfiguration<MaintenanceWindow>
{
    public void Configure(EntityTypeBuilder<MaintenanceWindow> builder)
    {
        builder.ToTable("maintenance_windows");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .ValueGeneratedNever();

        builder.Property(m => m.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(m => m.TitleAr)
            .HasColumnName("title_ar")
            .HasMaxLength(200);

        builder.Property(m => m.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(m => m.AffectedTechnology)
            .HasColumnName("affected_technology")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(m => m.StartTime)
            .HasColumnName("start_time")
            .IsRequired();

        builder.Property(m => m.EndTime)
            .HasColumnName("end_time")
            .IsRequired();

        builder.Property(m => m.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(m => m.ApprovedBy)
            .HasColumnName("approved_by")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(m => m.SuppressAlarms)
            .HasColumnName("suppress_alarms")
            .IsRequired();

        builder.Property(m => m.AffectedDeviceIds)
            .HasColumnName("affected_device_ids")
            .HasColumnType("text[]");

        builder.HasIndex(m => m.Status)
            .HasDatabaseName("ix_maintenance_windows_status");

        builder.HasIndex(m => m.AffectedTechnology)
            .HasDatabaseName("ix_maintenance_windows_affected_technology");

        builder.HasIndex(m => m.StartTime)
            .HasDatabaseName("ix_maintenance_windows_start_time");

        builder.HasIndex(m => m.EndTime)
            .HasDatabaseName("ix_maintenance_windows_end_time");
    }
}