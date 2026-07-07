using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Billing.Domain.Entities;

namespace Obss.Billing.Infrastructure.Persistence.Configurations;

public sealed class BillLineConfiguration : IEntityTypeConfiguration<BillLine>
{
    public void Configure(EntityTypeBuilder<BillLine> builder)
    {
        builder.ToTable("bill_lines");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id)
            .ValueGeneratedNever();

        builder.Property(l => l.BillId)
            .HasColumnName("bill_id")
            .IsRequired();

        builder.Property(l => l.LineType)
            .HasColumnName("line_type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(l => l.Description)
            .HasColumnName("description")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(l => l.SubscriptionId)
            .HasColumnName("subscription_id");

        builder.Property(l => l.ProductId)
            .HasColumnName("product_id");

        builder.Property(l => l.OfferId)
            .HasColumnName("offer_id");

        builder.Property(l => l.Quantity)
            .HasColumnName("quantity")
            .IsRequired();

        builder.Property(l => l.UnitPrice)
            .HasColumnName("unit_price")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(l => l.DiscountAmount)
            .HasColumnName("discount_amount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(l => l.TaxAmount)
            .HasColumnName("tax_amount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(l => l.TaxRate)
            .HasColumnName("tax_rate")
            .HasColumnType("decimal(5,4)")
            .IsRequired();

        builder.Property(l => l.TotalAmount)
            .HasColumnName("total_amount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(l => l.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(l => l.LineDate)
            .HasColumnName("line_date")
            .IsRequired();

        builder.Property(l => l.Reference)
            .HasColumnName("reference")
            .HasMaxLength(200);

        builder.Property(l => l.ExternalId)
            .HasColumnName("external_id")
            .HasMaxLength(100);

        builder.HasIndex(l => l.BillId)
            .HasDatabaseName("ix_bill_lines_bill_id");
    }
}
