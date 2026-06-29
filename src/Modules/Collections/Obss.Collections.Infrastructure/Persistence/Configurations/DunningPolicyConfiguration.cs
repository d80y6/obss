using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Collections.Domain.Entities;

namespace Obss.Collections.Infrastructure.Persistence.Configurations;

public sealed class DunningPolicyConfiguration : IEntityTypeConfiguration<DunningPolicy>
{
    public void Configure(EntityTypeBuilder<DunningPolicy> builder)
    {
        builder.ToTable("dunning_policies");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        builder.Property(p => p.TenantId)
            .HasColumnName("tenant_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(p => p.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(p => p.MaxDunningLevel)
            .HasColumnName("max_dunning_level")
            .IsRequired();

        builder.Property(p => p.DaysBetweenActions)
            .HasColumnName("days_between_actions")
            .IsRequired();

        builder.Property(p => p.EscalationAfterDays)
            .HasColumnName("escalation_after_days")
            .IsRequired();

        builder.Ignore(p => p.DunningFees);

        builder.HasIndex(p => p.TenantId)
            .HasDatabaseName("ix_dunning_policies_tenant_id");

        builder.HasIndex(p => p.IsActive)
            .HasDatabaseName("ix_dunning_policies_is_active");
    }
}
