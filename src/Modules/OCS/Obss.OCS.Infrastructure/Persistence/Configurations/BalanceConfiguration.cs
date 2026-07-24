using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.OCS.Domain.Entities;

namespace Obss.OCS.Infrastructure.Persistence.Configurations;

public sealed class BalanceConfiguration : IEntityTypeConfiguration<Balance>
{
    public void Configure(EntityTypeBuilder<Balance> builder)
    {
        builder.ToTable("ocs_balances");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).ValueGeneratedNever();
        builder.Property(b => b.TenantId).HasColumnName("tenant_id").HasMaxLength(100).IsRequired();
        builder.Property(b => b.SubscriptionId).HasColumnName("subscription_id").IsRequired();
        builder.Property(b => b.AvailableAmount).HasColumnName("available_amount").HasColumnType("decimal(18,4)").IsRequired();
        builder.Property(b => b.ReservedAmount).HasColumnName("reserved_amount").HasColumnType("decimal(18,4)").IsRequired();
        builder.Property(b => b.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        builder.Property(b => b.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(b => b.UpdatedAt).HasColumnName("updated_at").IsRequired();
        builder.Property(b => b.ConcurrencyStamp).HasColumnName("concurrency_stamp").HasDefaultValue(0).IsConcurrencyToken();
        builder.HasIndex(b => new { b.TenantId, b.SubscriptionId }).IsUnique().HasDatabaseName("ix_ocs_balances_tenant_subscription");
    }
}
