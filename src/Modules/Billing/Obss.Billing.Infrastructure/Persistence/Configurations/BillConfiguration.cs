using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Billing.Domain.Entities;

namespace Obss.Billing.Infrastructure.Persistence.Configurations;

public sealed class BillConfiguration : IEntityTypeConfiguration<Bill>
{
    public void Configure(EntityTypeBuilder<Bill> builder)
    {
        builder.ToTable("bills");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .ValueGeneratedNever();

        builder.Property(b => b.TenantId)
            .HasColumnName("tenant_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(b => b.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.Property(b => b.CustomerName)
            .HasColumnName("customer_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(b => b.BillingPeriod)
            .HasColumnName("billing_period")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(b => b.BillingPeriodStart)
            .HasColumnName("billing_period_start")
            .IsRequired();

        builder.Property(b => b.BillingPeriodEnd)
            .HasColumnName("billing_period_end")
            .IsRequired();

        builder.Property(b => b.DueDate)
            .HasColumnName("due_date")
            .IsRequired();

        builder.Property(b => b.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(b => b.SubTotal)
            .HasColumnName("sub_total")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(b => b.DiscountTotal)
            .HasColumnName("discount_total")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(b => b.TaxTotal)
            .HasColumnName("tax_total")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(b => b.GrandTotal)
            .HasColumnName("grand_total")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(b => b.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(b => b.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(b => b.FinalizedAt)
            .HasColumnName("finalized_at");

        builder.HasMany(b => b.Lines)
            .WithOne()
            .HasForeignKey(l => l.BillId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(b => b.CustomerId)
            .HasDatabaseName("ix_bills_customer_id");

        builder.HasIndex(b => b.Status)
            .HasDatabaseName("ix_bills_status");

        builder.HasIndex(b => b.TenantId)
            .HasDatabaseName("ix_bills_tenant_id");

        builder.HasIndex(b => new { b.CustomerId, b.Status })
            .HasDatabaseName("ix_bills_customer_id_status");

        builder.Navigation(b => b.Lines)
            .AutoInclude();
    }
}
