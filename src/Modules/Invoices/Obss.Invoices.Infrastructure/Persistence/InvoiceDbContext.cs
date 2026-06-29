using Microsoft.EntityFrameworkCore;
using Obss.Invoices.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.Invoices.Infrastructure.Persistence;

public class InvoiceDbContext : EfDbContext
{
    public InvoiceDbContext(
        DbContextOptions<InvoiceDbContext> options,
        ICurrentTenant currentTenant,
        IDomainEventDispatcher domainEventDispatcher)
        : base(options, currentTenant, domainEventDispatcher)
    {
    }

    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();
    public DbSet<InvoicePayment> InvoicePayments => Set<InvoicePayment>();
    public DbSet<InvoiceNote> InvoiceNotes => Set<InvoiceNote>();
    public DbSet<CreditNote> CreditNotes => Set<CreditNote>();
    public DbSet<CreditNoteLine> CreditNoteLines => Set<CreditNoteLine>();
    public DbSet<InvoiceDispute> InvoiceDisputes => Set<InvoiceDispute>();
    public DbSet<DisputeAttachment> DisputeAttachments => Set<DisputeAttachment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InvoiceDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
