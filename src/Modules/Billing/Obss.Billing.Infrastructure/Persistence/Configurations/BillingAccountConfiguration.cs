using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Billing.Domain.Entities;

namespace Obss.Billing.Infrastructure.Persistence.Configurations;

public sealed class BillingAccountConfiguration : IEntityTypeConfiguration<BillingAccount>
{
    public void Configure(EntityTypeBuilder<BillingAccount> builder)
    {
        builder.ToTable("billing_accounts");

        builder.HasKey(ba => ba.Id);

        builder.Property(ba => ba.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(ba => ba.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.Property(ba => ba.AccountType)
            .HasColumnName("account_type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(ba => ba.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(ba => ba.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(ba => ba.CreditLimit)
            .HasColumnName("credit_limit")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(ba => ba.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(ba => ba.ValidFrom)
            .HasColumnName("valid_from");

        builder.Property(ba => ba.ValidUntil)
            .HasColumnName("valid_until");

        builder.Property(ba => ba.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(ba => ba.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(ba => ba.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(ba => ba.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(ba => ba.CustomerId)
            .HasDatabaseName("ix_billing_accounts_customer_id");
    }
}
