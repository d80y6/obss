using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Billing.Domain.Entities;

namespace Obss.Billing.Infrastructure.Persistence.Configurations;

public sealed class BillingCycleConfiguration : IEntityTypeConfiguration<BillingCycle>
{
    public void Configure(EntityTypeBuilder<BillingCycle> builder)
    {
        builder.ToTable("billing_cycles");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .ValueGeneratedNever();

        builder.Property(c => c.TenantId)
            .HasColumnName("tenant_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.Property(c => c.BillingPeriod)
            .HasColumnName("billing_period")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(c => c.LastBillingDate)
            .HasColumnName("last_billing_date")
            .IsRequired();

        builder.Property(c => c.NextBillingDate)
            .HasColumnName("next_billing_date")
            .IsRequired();

        builder.Property(c => c.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(c => c.CustomerId)
            .HasDatabaseName("ix_billing_cycles_customer_id");

        builder.HasIndex(c => c.NextBillingDate)
            .HasDatabaseName("ix_billing_cycles_next_billing_date");
    }
}
