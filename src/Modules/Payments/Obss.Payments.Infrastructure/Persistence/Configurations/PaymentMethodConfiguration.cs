using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Payments.Domain.Entities;

namespace Obss.Payments.Infrastructure.Persistence.Configurations;

public sealed class PaymentMethodConfiguration : IEntityTypeConfiguration<PaymentMethod>
{
    public void Configure(EntityTypeBuilder<PaymentMethod> builder)
    {
        builder.ToTable("payment_methods");

        builder.HasKey(pm => pm.Id);

        builder.Property(pm => pm.Id)
            .ValueGeneratedNever();

        builder.Property(pm => pm.TenantId)
            .HasColumnName("tenant_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(pm => pm.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.Property(pm => pm.MethodType)
            .HasColumnName("method_type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(pm => pm.IsDefault)
            .HasColumnName("is_default")
            .IsRequired();

        builder.Property(pm => pm.Provider)
            .HasColumnName("provider")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(pm => pm.AccountNumber)
            .HasColumnName("account_number")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(pm => pm.ExpiryDate)
            .HasColumnName("expiry_date");

        builder.Property(pm => pm.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(pm => pm.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(pm => new { pm.TenantId, pm.CustomerId })
            .HasDatabaseName("ix_payment_methods_tenant_id_customer_id");
    }
}
