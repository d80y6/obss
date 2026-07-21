using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.OCS.Domain.Entities;
using Obss.OCS.Domain.ValueObjects;

namespace Obss.OCS.Infrastructure.Persistence.Configurations;

public sealed class CreditPoolConfiguration : IEntityTypeConfiguration<CreditPool>
{
    public void Configure(EntityTypeBuilder<CreditPool> builder)
    {
        builder.ToTable("ocs_credit_pools");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();
        builder.Property(p => p.TenantId).HasColumnName("tenant_id").HasMaxLength(100).IsRequired();
        builder.Property(p => p.SubscriptionId).HasColumnName("subscription_id").IsRequired();
        builder.Property(p => p.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(p => p.TotalAmount).HasColumnName("total_amount").HasColumnType("decimal(18,4)").IsRequired();
        builder.Property(p => p.RemainingAmount).HasColumnName("remaining_amount").HasColumnType("decimal(18,4)").IsRequired();
        builder.Property(p => p.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        builder.Property(p => p.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(p => p.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(p => p.ExpiresAt).HasColumnName("expires_at");
        builder.HasIndex(p => new { p.TenantId, p.SubscriptionId }).HasDatabaseName("ix_ocs_credit_pools_tenant_subscription");
    }
}
