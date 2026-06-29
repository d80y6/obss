using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Workflow.Domain.Entities;

namespace Obss.Workflow.Infrastructure.Persistence.Configurations;

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

        builder.Property(s => s.TargetType)
            .HasColumnName("target_type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.TargetDuration)
            .HasColumnName("target_duration")
            .IsRequired();

        builder.Property(s => s.EscalationDuration)
            .HasColumnName("escalation_duration")
            .IsRequired();

        builder.Property(s => s.PenaltyAmount)
            .HasColumnName("penalty_amount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(s => s.PenaltyCurrency)
            .HasColumnName("penalty_currency")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(s => s.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(s => new { s.TenantId, s.Name })
            .HasDatabaseName("ix_sla_definitions_tenant_name")
            .IsUnique();
    }
}
