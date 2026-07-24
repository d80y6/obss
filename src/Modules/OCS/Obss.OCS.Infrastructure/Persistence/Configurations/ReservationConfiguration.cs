using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.OCS.Domain.Entities;

namespace Obss.OCS.Infrastructure.Persistence.Configurations;

public sealed class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("ocs_reservations");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();
        builder.Property(r => r.TenantId).HasColumnName("tenant_id").HasMaxLength(100).IsRequired();
        builder.Property(r => r.BalanceId).HasColumnName("balance_id").IsRequired();
        builder.Property(r => r.SubscriptionId).HasColumnName("subscription_id").IsRequired();
        builder.Property(r => r.Amount).HasColumnName("amount").HasColumnType("decimal(18,4)").IsRequired();
        builder.Property(r => r.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        builder.Property(r => r.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(r => r.ReservedAt).HasColumnName("reserved_at").IsRequired();
        builder.Property(r => r.ExpiresAt).HasColumnName("expires_at").IsRequired();
        builder.Property(r => r.UpdatedAt).HasColumnName("updated_at").IsRequired();
        builder.HasIndex(r => r.Status).HasDatabaseName("ix_ocs_reservations_status");
        builder.HasIndex(r => new { r.TenantId, r.SubscriptionId }).HasDatabaseName("ix_ocs_reservations_tenant_subscription");
    }
}
