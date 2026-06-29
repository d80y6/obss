using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Invoices.Domain.Entities;

namespace Obss.Invoices.Infrastructure.Persistence.Configurations;

public sealed class InvoiceDisputeConfiguration : IEntityTypeConfiguration<InvoiceDispute>
{
    public void Configure(EntityTypeBuilder<InvoiceDispute> builder)
    {
        builder.ToTable("invoice_disputes");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
            .ValueGeneratedNever();

        builder.Property(d => d.InvoiceId)
            .HasColumnName("invoice_id")
            .IsRequired();

        builder.Property(d => d.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.Property(d => d.Reason)
            .HasColumnName("reason")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(d => d.Description)
            .HasColumnName("description")
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(d => d.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(d => d.DisputedAmount)
            .HasColumnName("disputed_amount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(d => d.Resolution)
            .HasColumnName("resolution")
            .HasMaxLength(2000);

        builder.Property(d => d.ResolvedById)
            .HasColumnName("resolved_by_id");

        builder.Property(d => d.ResolvedAt)
            .HasColumnName("resolved_at");

        builder.Property(d => d.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(d => d.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasMany(d => d.Attachments)
            .WithOne()
            .HasForeignKey("InvoiceDisputeId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(d => d.InvoiceId)
            .HasDatabaseName("ix_invoice_disputes_invoice_id");

        builder.HasIndex(d => d.Status)
            .HasDatabaseName("ix_invoice_disputes_status");

        builder.Ignore(d => d.DomainEvents);
        builder.Ignore(d => d.IntegrationEvents);
    }
}
