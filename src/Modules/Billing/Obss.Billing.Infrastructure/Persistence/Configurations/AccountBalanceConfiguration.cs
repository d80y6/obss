using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Billing.Domain.Entities;

namespace Obss.Billing.Infrastructure.Persistence.Configurations;

public sealed class AccountBalanceConfiguration : IEntityTypeConfiguration<AccountBalance>
{
    public void Configure(EntityTypeBuilder<AccountBalance> builder)
    {
        builder.ToTable("account_balances");

        builder.HasKey(ab => ab.Id);

        builder.Property(ab => ab.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(ab => ab.TenantId)
            .HasColumnName("tenant_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(ab => ab.BillingAccountId)
            .HasColumnName("billing_account_id")
            .IsRequired();

        builder.Property(ab => ab.CurrentBalance)
            .HasColumnName("current_balance")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(ab => ab.OutstandingBalance)
            .HasColumnName("outstanding_balance")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(ab => ab.AvailableCredit)
            .HasColumnName("available_credit")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(ab => ab.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(ab => ab.BalanceDate)
            .HasColumnName("balance_date")
            .IsRequired();

        builder.Property(ab => ab.LastUpdatedAt)
            .HasColumnName("last_updated_at")
            .IsRequired();

        builder.Property(ab => ab.AtType)
            .HasColumnName("at_type")
            .HasMaxLength(100);

        builder.Property(ab => ab.AtBaseType)
            .HasColumnName("at_base_type")
            .HasMaxLength(100);

        builder.Property(ab => ab.AtSchemaLocation)
            .HasColumnName("at_schema_location")
            .HasMaxLength(500);

        builder.OwnsMany(ab => ab.Transactions, t =>
        {
            t.ToTable("balance_transactions");
            t.WithOwner().HasForeignKey("balance_id");
            t.HasKey("Id");

            t.Property(t => t.Id)
                .HasColumnName("id")
                .ValueGeneratedNever();

            t.Property(t => t.Amount)
                .HasColumnName("amount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            t.Property(t => t.TransactionType)
                .HasColumnName("transaction_type")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            t.Property(t => t.Description)
                .HasColumnName("description")
                .HasMaxLength(500)
                .IsRequired();

            t.Property(t => t.TransactionDate)
                .HasColumnName("transaction_date")
                .IsRequired();

            t.Property(t => t.ReferenceId)
                .HasColumnName("reference_id")
                .HasMaxLength(100);

            t.Property(t => t.ReferenceType)
                .HasColumnName("reference_type")
                .HasMaxLength(50);
        });

        builder.Navigation(ab => ab.Transactions)
            .AutoInclude();

        builder.HasIndex(ab => ab.BillingAccountId)
            .HasDatabaseName("ix_account_balances_billing_account_id");

        builder.HasIndex(ab => ab.BalanceDate)
            .HasDatabaseName("ix_account_balances_balance_date");
    }
}
