using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Invoices.Domain.Entities;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.Invoices.Infrastructure.Persistence.Configurations;

public sealed class CreditNoteConfiguration : IEntityTypeConfiguration<CreditNote>
{
    public void Configure(EntityTypeBuilder<CreditNote> builder)
    {
        builder.ToTable("credit_notes");

        builder.HasKey(cn => cn.Id);

        builder.Property(cn => cn.Id)
            .ValueGeneratedNever();

        builder.Property(cn => cn.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion<TenantIdValueConverter>()
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(cn => cn.CreditNoteNumber)
            .HasColumnName("credit_note_number")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(cn => cn.InvoiceId)
            .HasColumnName("invoice_id")
            .IsRequired();

        builder.Property(cn => cn.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.Property(cn => cn.Reason)
            .HasColumnName("reason")
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(cn => cn.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(cn => cn.SubTotal)
            .HasColumnName("sub_total")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(cn => cn.TaxAmount)
            .HasColumnName("tax_amount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(cn => cn.TotalAmount)
            .HasColumnName("total_amount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(cn => cn.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(cn => cn.IssuedAt)
            .HasColumnName("issued_at")
            .IsRequired();

        builder.Property(cn => cn.AppliedAt)
            .HasColumnName("applied_at");

        builder.Property(cn => cn.ExternalId)
            .HasColumnName("external_id")
            .HasMaxLength(100);

        builder.HasMany(cn => cn.Lines)
            .WithOne(cl => cl.CreditNote)
            .HasForeignKey(cl => cl.CreditNoteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(cn => cn.CreditNoteNumber)
            .HasDatabaseName("ix_credit_notes_credit_note_number")
            .IsUnique();

        builder.HasIndex(cn => cn.InvoiceId)
            .HasDatabaseName("ix_credit_notes_invoice_id");

        builder.HasIndex(c => new { c.TenantId, c.CreditNoteNumber })
            .HasDatabaseName("ix_credit_notes_tenant_id_number")
            .IsUnique();

        builder.Ignore(cn => cn.DomainEvents);
        builder.Ignore(cn => cn.IntegrationEvents);
    }
}
