using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Payments.Domain.Entities;
using Obss.Payments.Domain.ValueObjects;

namespace Obss.Payments.Infrastructure.Persistence.Configurations;

public sealed class PaymentGatewayConfiguration : IEntityTypeConfiguration<PaymentGateway>
{
    public void Configure(EntityTypeBuilder<PaymentGateway> builder)
    {
        builder.ToTable("payment_gateways");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.Id)
            .ValueGeneratedNever();

        builder.Property(g => g.TenantId)
            .HasColumnName("tenant_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(g => g.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(g => g.Provider)
            .HasColumnName("provider")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(g => g.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(g => g.Configuration)
            .HasColumnName("configuration")
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(g => g.SupportedCurrencies)
            .HasColumnName("supported_currencies")
            .HasMaxLength(500)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() as IReadOnlyCollection<string>)
            .Metadata.SetValueComparer(new ValueComparer<IReadOnlyCollection<string>>(
                (c1, c2) => c1!.SequenceEqual(c2!),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList()));

        builder.Property(g => g.MinAmount)
            .HasColumnName("min_amount")
            .HasPrecision(18, 2);

        builder.Property(g => g.MaxAmount)
            .HasColumnName("max_amount")
            .HasPrecision(18, 2);

        builder.Property(g => g.TransactionFee)
            .HasColumnName("transaction_fee")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(g => g.FeeType)
            .HasColumnName("fee_type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(g => g.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(g => new { g.TenantId, g.Provider })
            .HasDatabaseName("ix_payment_gateways_tenant_id_provider")
            .IsUnique();

        builder.HasIndex(g => g.IsActive)
            .HasDatabaseName("ix_payment_gateways_is_active");
    }
}
