using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Ticketing.Domain.Entities;

namespace Obss.Ticketing.Infrastructure.Persistence.Configurations;

public sealed class SlaDefinitionConfiguration : IEntityTypeConfiguration<SlaDefinition>
{
    public void Configure(EntityTypeBuilder<SlaDefinition> builder)
    {
        builder.ToTable("sla_definitions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedNever();

        builder.Property(s => s.TenantId)
            .HasColumnName("tenant_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(s => s.Priority)
            .HasColumnName("priority")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(s => s.ResponseTimeHours)
            .HasColumnName("response_time_hours")
            .IsRequired();

        builder.Property(s => s.ResolutionTimeHours)
            .HasColumnName("resolution_time_hours")
            .IsRequired();

        builder.Property(s => s.EscalationTimeHours)
            .HasColumnName("escalation_time_hours")
            .IsRequired();

        builder.Property(s => s.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(s => s.TenantId)
            .HasDatabaseName("ix_sla_definitions_tenant_id");

        builder.HasIndex(s => new { s.TenantId, s.Priority, s.IsActive })
            .HasDatabaseName("ix_sla_definitions_tenant_priority_active");
    }
}
