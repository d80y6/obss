using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Invoices.Domain.Entities;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.Invoices.Infrastructure.Persistence.Configurations;

public sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("invoices");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .ValueGeneratedNever();

        builder.Property(i => i.TenantId)
            .HasColumnName("tenant_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(i => i.InvoiceNumber)
            .HasColumnName("invoice_number")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(i => i.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.Property(i => i.CustomerName)
            .HasColumnName("customer_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(i => i.CustomerEmail)
            .HasColumnName("customer_email")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(i => i.CustomerAddress)
            .HasColumnName("customer_address")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(i => i.InvoiceDate)
            .HasColumnName("invoice_date")
            .IsRequired();

        builder.Property(i => i.DueDate)
            .HasColumnName("due_date")
            .IsRequired();

        builder.Property(i => i.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(i => i.SubTotal)
            .HasColumnName("sub_total")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(i => i.DiscountTotal)
            .HasColumnName("discount_total")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(i => i.TaxTotal)
            .HasColumnName("tax_total")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(i => i.GrandTotal)
            .HasColumnName("grand_total")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(i => i.AmountPaid)
            .HasColumnName("amount_paid")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(i => i.AmountDue)
            .HasColumnName("amount_due")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(i => i.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(i => i.Notes)
            .HasColumnName("notes")
            .HasMaxLength(2000);

        builder.Property(i => i.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(i => i.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Property(i => i.SentAt)
            .HasColumnName("sent_at");

        builder.Property(i => i.PaidAt)
            .HasColumnName("paid_at");

        builder.Property(i => i.Href)
            .HasColumnName("href")
            .HasMaxLength(500);

        builder.Property(i => i.AtType)
            .HasColumnName("at_type")
            .HasMaxLength(100);

        builder.Property(i => i.AtBaseType)
            .HasColumnName("at_base_type")
            .HasMaxLength(100);

        builder.Property(i => i.AtSchemaLocation)
            .HasColumnName("at_schema_location")
            .HasMaxLength(500);

        builder.Property(i => i.ExternalId)
            .HasColumnName("external_id")
            .HasMaxLength(100);

        builder.OwnsMany(i => i.RelatedParties, rp =>
        {
            rp.ToTable("invoice_related_parties");
            rp.WithOwner().HasForeignKey("invoice_id");
            rp.Property(r => r.PartyId).HasColumnName("party_id").HasMaxLength(100);
            rp.Property(r => r.PartyName).HasColumnName("party_name").HasMaxLength(200);
            rp.Property(r => r.Role).HasColumnName("role").HasMaxLength(50);
        });

        builder.HasMany(i => i.Lines)
            .WithOne(l => l.Invoice)
            .HasForeignKey(l => l.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(i => i.Payments)
            .WithOne()
            .HasForeignKey("InvoiceId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(i => i.NotesCollection)
            .WithOne()
            .HasForeignKey("InvoiceId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(i => i.InvoiceNumber)
            .HasDatabaseName("ix_invoices_invoice_number")
            .IsUnique();

        builder.HasIndex(i => i.CustomerId)
            .HasDatabaseName("ix_invoices_customer_id");

        builder.HasIndex(i => i.Status)
            .HasDatabaseName("ix_invoices_status");

        builder.HasIndex(i => i.DueDate)
            .HasDatabaseName("ix_invoices_due_date");

        builder.HasIndex(i => new { i.TenantId, i.InvoiceNumber })
            .HasDatabaseName("ix_invoices_tenant_id_invoice_number")
            .IsUnique();

        builder.HasIndex(i => new { i.TenantId, i.CustomerId })
            .HasDatabaseName("ix_invoices_tenant_id_customer_id");

        builder.HasIndex(i => new { i.TenantId, i.Status })
            .HasDatabaseName("ix_invoices_tenant_id_status");

        builder.Ignore(i => i.DomainEvents);
        builder.Ignore(i => i.IntegrationEvents);
    }
}
