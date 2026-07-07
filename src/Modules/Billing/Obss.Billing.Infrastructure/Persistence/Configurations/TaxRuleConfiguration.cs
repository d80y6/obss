using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Billing.Domain.Entities;

namespace Obss.Billing.Infrastructure.Persistence.Configurations;

public sealed class TaxRuleConfiguration : IEntityTypeConfiguration<TaxRule>
{
    public void Configure(EntityTypeBuilder<TaxRule> builder)
    {
        builder.ToTable("tax_rules");

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
            .HasMaxLength(500);

        builder.Property(r => r.TaxRate)
            .HasColumnName("tax_rate")
            .HasColumnType("decimal(18,6)")
            .IsRequired();

        builder.Property(r => r.TaxType)
            .HasColumnName("tax_type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(r => r.TaxCategory)
            .HasColumnName("tax_category")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(r => r.Country)
            .HasColumnName("country")
            .HasMaxLength(10)
            .HasDefaultValue("YE");

        builder.Property(r => r.Region)
            .HasColumnName("region")
            .HasMaxLength(100);

        builder.Property(r => r.IsCompound)
            .HasColumnName("is_compound")
            .IsRequired();

        builder.Property(r => r.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(r => r.Priority)
            .HasColumnName("priority")
            .IsRequired();

        builder.Property(r => r.EffectiveFrom)
            .HasColumnName("effective_from")
            .IsRequired();

        builder.Property(r => r.EffectiveTo)
            .HasColumnName("effective_to");

        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(r => r.ExternalId)
            .HasColumnName("external_id")
            .HasMaxLength(100);

        builder.HasIndex(r => r.TenantId)
            .HasDatabaseName("ix_tax_rules_tenant_id");

        builder.HasIndex(r => new { r.TaxCategory, r.Country })
            .HasDatabaseName("ix_tax_rules_category_country");

        builder.HasIndex(r => r.IsActive)
            .HasDatabaseName("ix_tax_rules_is_active");
    }
}
