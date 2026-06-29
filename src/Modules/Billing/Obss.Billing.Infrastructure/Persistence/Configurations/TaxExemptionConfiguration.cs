using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Billing.Domain.Entities;

namespace Obss.Billing.Infrastructure.Persistence.Configurations;

public sealed class TaxExemptionConfiguration : IEntityTypeConfiguration<TaxExemption>
{
    public void Configure(EntityTypeBuilder<TaxExemption> builder)
    {
        builder.ToTable("tax_exemptions");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedNever();

        builder.Property(e => e.TenantId)
            .HasColumnName("tenant_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.Property(e => e.TaxRuleId)
            .HasColumnName("tax_rule_id")
            .IsRequired();

        builder.Property(e => e.ExemptionCertificate)
            .HasColumnName("exemption_certificate")
            .HasMaxLength(200);

        builder.Property(e => e.ExemptionRate)
            .HasColumnName("exemption_rate")
            .HasColumnType("decimal(18,6)")
            .IsRequired();

        builder.Property(e => e.ValidFrom)
            .HasColumnName("valid_from")
            .IsRequired();

        builder.Property(e => e.ValidTo)
            .HasColumnName("valid_to")
            .IsRequired();

        builder.Property(e => e.ApprovedBy)
            .HasColumnName("approved_by")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(e => e.CustomerId)
            .HasDatabaseName("ix_tax_exemptions_customer_id");

        builder.HasIndex(e => new { e.CustomerId, e.TaxRuleId })
            .HasDatabaseName("ix_tax_exemptions_customer_tax_rule");
    }
}
