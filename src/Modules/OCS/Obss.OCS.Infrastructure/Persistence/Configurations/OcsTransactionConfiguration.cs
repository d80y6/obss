using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.OCS.Domain.Entities;

namespace Obss.OCS.Infrastructure.Persistence.Configurations;

public sealed class OcsTransactionConfiguration : IEntityTypeConfiguration<OcsTransaction>
{
    public void Configure(EntityTypeBuilder<OcsTransaction> builder)
    {
        builder.ToTable("ocs_transactions");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();
        builder.Property(t => t.TenantId).HasColumnName("tenant_id").HasMaxLength(100).IsRequired();
        builder.Property(t => t.SubscriptionId).HasColumnName("subscription_id").IsRequired();
        builder.Property(t => t.BalanceId).HasColumnName("balance_id");
        builder.Property(t => t.CreditPoolId).HasColumnName("credit_pool_id");
        builder.Property(t => t.TransactionType).HasColumnName("transaction_type").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(t => t.Amount).HasColumnName("amount").HasColumnType("decimal(18,4)").IsRequired();
        builder.Property(t => t.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        builder.Property(t => t.Description).HasColumnName("description").HasMaxLength(500);
        builder.Property(t => t.Timestamp).HasColumnName("timestamp").IsRequired();
        builder.HasIndex(t => new { t.TenantId, t.SubscriptionId, t.Timestamp }).HasDatabaseName("ix_ocs_transactions_subscription_time");
    }
}
