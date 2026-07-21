using Xunit;
using FluentAssertions;
using Obss.Invoices.Domain.Entities;
using Obss.Invoices.Domain.ValueObjects;
using Obss.Invoices.Infrastructure.Persistence.Repositories;
namespace Obss.Invoices.Tests;

public class RepositoryTests : InvoiceIntegrationTests
{
    [Fact]
    public async Task CanAddAndRetrieveInvoice()
    {
        using var context = CreateDbContext();
        var repository = new InvoiceRepository(context);

        var tenantId = Guid.NewGuid().ToString("N");
        var customerId = Guid.NewGuid();
        var invoice = Invoice.Create(
            tenantId,
            "INV-2026-00001",
            customerId,
            "Test Customer",
            "customer@example.com",
            "123 Test St",
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(30),
            "USD");

        await repository.AddAsync(invoice);
        await context.SaveChangesAsync();

        var retrieved = await repository.GetByIdAsync(invoice.Id);

        retrieved.Should().NotBeNull();
        retrieved!.InvoiceNumber.Should().Be("INV-2026-00001");
        retrieved.CustomerName.Should().Be("Test Customer");
        retrieved.CustomerEmail.Should().Be("customer@example.com");
        retrieved.Status.Should().Be(InvoiceStatus.Draft);
    }

    [Fact]
    public async Task CanAddInvoiceWithLines()
    {
        using var context = CreateDbContext();
        var repository = new InvoiceRepository(context);

        var tenantId = Guid.NewGuid().ToString("N");
        var customerId = Guid.NewGuid();
        var invoice = Invoice.Create(
            tenantId,
            "INV-2026-00002",
            customerId,
            "Line Customer",
            "line@example.com",
            "456 Line Ave",
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(30),
            "USD");

        var line1 = new InvoiceLine(
            Guid.NewGuid(),
            invoice.Id,
            Guid.NewGuid(),
            null,
            LineType.OneTime,
            "Setup fee",
            1,
            100.00m,
            100.00m,
            10.00m,
            0.10m,
            "USD");

        var line2 = new InvoiceLine(
            Guid.NewGuid(),
            invoice.Id,
            Guid.NewGuid(),
            null,
            LineType.Recurring,
            "Monthly subscription",
            1,
            49.99m,
            49.99m,
            5.00m,
            0.10m,
            "USD");

        invoice.AddLines([line1, line2]);
        await repository.AddAsync(invoice);
        await context.SaveChangesAsync();

        var retrieved = await repository.GetByIdWithDetailsAsync(invoice.Id);

        retrieved.Should().NotBeNull();
        retrieved!.Lines.Should().HaveCount(2);
        retrieved.SubTotal.Should().Be(149.99m);
        retrieved.GrandTotal.Should().Be(164.99m);
    }

    [Fact]
    public async Task CanGetInvoicesByCustomer()
    {
        using var context = CreateDbContext();
        var repository = new InvoiceRepository(context);
        var tenantId = Guid.NewGuid().ToString("N");
        var customerId = Guid.NewGuid();

        for (var i = 1; i <= 3; i++)
        {
            var invoice = Invoice.Create(
                tenantId,
                $"INV-2026-{i:D5}",
                customerId,
                "Customer A",
                "a@example.com",
                "Address",
                DateTime.UtcNow,
                DateTime.UtcNow.AddDays(30),
                "USD");
            await repository.AddAsync(invoice);
        }
        await context.SaveChangesAsync();

        var invoices = await repository.GetByCustomerAsync(customerId);
        invoices.Should().HaveCount(3);
    }

    [Fact]
    public async Task CanGenerateNextInvoiceNumber()
    {
        using var context = CreateDbContext();
        var repository = new InvoiceRepository(context);
        var tenantId = Guid.NewGuid().ToString("N");

        var firstNumber = await repository.GenerateNextInvoiceNumberAsync();
        firstNumber.Should().Match("INV-2026-*");

        var invoice = Invoice.Create(
            tenantId,
            firstNumber,
            Guid.NewGuid(),
            "Num Test",
            "num@example.com",
            "Addr",
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(30),
            "USD");

        await repository.AddAsync(invoice);
        await context.SaveChangesAsync();

        var secondNumber = await repository.GenerateNextInvoiceNumberAsync();
        var parts = firstNumber.Split('-');
        var sequence = int.Parse(parts[^1]);
        secondNumber.Should().Be($"INV-2026-{sequence + 1:D5}");
    }

    [Fact]
    public async Task CanGetInvoiceSummary()
    {
        using var context = CreateDbContext();
        var repository = new InvoiceRepository(context);
        var tenantId = Guid.NewGuid().ToString("N");
        var customerId = Guid.NewGuid();

        var invoice1 = Invoice.Create(tenantId, "INV-2026-01001", customerId, "S1", "s1@example.com", "Addr",
            DateTime.UtcNow, DateTime.UtcNow.AddDays(30), "USD");
        var invoice2 = Invoice.Create(tenantId, "INV-2026-01002", customerId, "S2", "s2@example.com", "Addr",
            DateTime.UtcNow, DateTime.UtcNow.AddDays(30), "USD");

        await repository.AddAsync(invoice1);
        await repository.AddAsync(invoice2);
        await context.SaveChangesAsync();

        var summary = await repository.GetInvoiceSummaryAsync();

        summary.TotalInvoices.Should().Be(2);
        summary.DraftCount.Should().Be(2);
    }

    [Fact]
    public async Task CanGetOverdueInvoices()
    {
        using var context = CreateDbContext();
        var repository = new InvoiceRepository(context);
        var tenantId = Guid.NewGuid().ToString("N");
        var customerId = Guid.NewGuid();

        var invoice = Invoice.Create(
            tenantId, "INV-2026-02001", customerId, "Overdue Customer",
            "overdue@example.com", "Addr",
            DateTime.UtcNow.AddDays(-60), DateTime.UtcNow.AddDays(-30), "USD");

        var line = new InvoiceLine(Guid.NewGuid(), invoice.Id, Guid.NewGuid(), null,
            LineType.OneTime, "Fee", 1, 100m, 100m, 10m, 0.10m, "USD");
        invoice.AddLine(line);
        invoice.MarkAsFinalized();
        invoice.MarkAsSent();

        await repository.AddAsync(invoice);
        await context.SaveChangesAsync();

        var overdueInvoices = await repository.GetOverdueInvoicesAsync();

        overdueInvoices.Should().Contain(i => i.Id == invoice.Id);
    }

    [Fact]
    public async Task CanGetInvoicesByCustomerWithStatusFilter()
    {
        using var context = CreateDbContext();
        var repository = new InvoiceRepository(context);
        var tenantId = Guid.NewGuid().ToString("N");
        var customerId = Guid.NewGuid();

        var invoice1 = Invoice.Create(tenantId, "INV-2026-03001", customerId, "Filter A",
            "fA@example.com", "Addr", DateTime.UtcNow, DateTime.UtcNow.AddDays(30), "USD");
        var invoice2 = Invoice.Create(tenantId, "INV-2026-03002", customerId, "Filter B",
            "fB@example.com", "Addr", DateTime.UtcNow, DateTime.UtcNow.AddDays(30), "USD");

        invoice2.AddLine(new InvoiceLine(Guid.NewGuid(), invoice2.Id, Guid.NewGuid(), null,
            LineType.OneTime, "Fee", 1, 50m, 50m, 5m, 0.10m, "USD"));
        invoice2.MarkAsFinalized();

        await repository.AddAsync(invoice1);
        await repository.AddAsync(invoice2);
        await context.SaveChangesAsync();

        var draftInvoices = await repository.GetByCustomerAsync(customerId, InvoiceStatus.Draft);
        draftInvoices.Should().HaveCount(1);
        draftInvoices[0].Id.Should().Be(invoice1.Id);

        var finalizedInvoices = await repository.GetByCustomerAsync(customerId, InvoiceStatus.Finalized);
        finalizedInvoices.Should().HaveCount(1);
        finalizedInvoices[0].Id.Should().Be(invoice2.Id);
    }

    [Fact]
    public async Task CanAddAndRetrieveCreditNote()
    {
        using var context = CreateDbContext();
        var invoiceRepository = new InvoiceRepository(context);

        var tenantId = Guid.NewGuid().ToString("N");
        var invoice = Invoice.Create(tenantId, "INV-2026-04001", Guid.NewGuid(),
            "CN Customer", "cn@example.com", "Addr",
            DateTime.UtcNow, DateTime.UtcNow.AddDays(30), "USD");

        await invoiceRepository.AddAsync(invoice);
        await context.SaveChangesAsync();

        var creditNote = CreditNote.Create(tenantId, "CN-2026-00001", invoice.Id,
            invoice.CustomerId, "Refund", "USD");

        var line = new CreditNoteLine(Guid.NewGuid(), creditNote.Id, Guid.NewGuid(),
            "Refund line", 100m, 1);
        creditNote.AddLine(line);
        creditNote.Issue();

        await invoiceRepository.AddCreditNoteAsync(creditNote);
        await context.SaveChangesAsync();

        var retrieved = await invoiceRepository.GetCreditNoteByIdAsync(creditNote.Id);

        retrieved.Should().NotBeNull();
        retrieved!.CreditNoteNumber.Should().Be("CN-2026-00001");
        retrieved.Status.Should().Be(CreditNoteStatus.Issued);
        retrieved.Lines.Should().HaveCount(1);
        retrieved.TotalAmount.Should().Be(100m);
    }

    [Fact]
    public async Task CanAddAndRetrieveDispute()
    {
        using var context = CreateDbContext();
        var disputeRepository = new InvoiceDisputeRepository(context);

        var dispute = InvoiceDispute.Submit(
            Guid.NewGuid(), Guid.NewGuid(), "Billing error",
            "Amount charged is incorrect", 250m);

        dispute.AddAttachment("evidence.pdf", "application/pdf", 2048, "/uploads/evidence.pdf");

        await disputeRepository.AddAsync(dispute);
        await context.SaveChangesAsync();

        var retrieved = await disputeRepository.GetByIdWithAttachmentsAsync(dispute.Id);

        retrieved.Should().NotBeNull();
        retrieved!.Reason.Should().Be("Billing error");
        retrieved.Status.Should().Be(DisputeStatus.Open);
        retrieved.Attachments.Should().HaveCount(1);
    }

    [Fact]
    public async Task CanGetDisputesByInvoiceId()
    {
        using var context = CreateDbContext();
        var disputeRepository = new InvoiceDisputeRepository(context);

        var invoiceId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var dispute1 = InvoiceDispute.Submit(invoiceId, customerId, "Reason 1", "Desc 1", 100m);
        var dispute2 = InvoiceDispute.Submit(invoiceId, customerId, "Reason 2", "Desc 2", 200m);
        var dispute3 = InvoiceDispute.Submit(Guid.NewGuid(), customerId, "Other", "Desc 3", 300m);

        await disputeRepository.AddAsync(dispute1);
        await disputeRepository.AddAsync(dispute2);
        await disputeRepository.AddAsync(dispute3);
        await context.SaveChangesAsync();

        var disputes = await disputeRepository.GetDisputesAsync(invoiceId, null);

        disputes.Should().HaveCount(2);
        disputes.All(d => d.InvoiceId == invoiceId).Should().BeTrue();
    }

    [Fact]
    public async Task CanGetDisputesByStatus()
    {
        using var context = CreateDbContext();
        var disputeRepository = new InvoiceDisputeRepository(context);

        var invoiceId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var openDispute = InvoiceDispute.Submit(invoiceId, customerId, "Open", "Open dispute", 100m);
        var resolvedDispute = InvoiceDispute.Submit(invoiceId, customerId, "Resolved", "Resolved dispute", 200m);
        resolvedDispute.AcceptResolution("Credit", Guid.NewGuid());

        await disputeRepository.AddAsync(openDispute);
        await disputeRepository.AddAsync(resolvedDispute);
        await context.SaveChangesAsync();

        var openDisputes = await disputeRepository.GetDisputesAsync(null, "Open");
        openDisputes.Should().Contain(d => d.Id == openDispute.Id);
        openDisputes.Should().NotContain(d => d.Id == resolvedDispute.Id);
    }
}
