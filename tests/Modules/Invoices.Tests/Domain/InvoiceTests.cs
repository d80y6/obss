using Xunit;
using FluentAssertions;
using Obss.Invoices.Domain.Entities;
using Obss.Invoices.Domain.Exceptions;
using Obss.Invoices.Domain.ValueObjects;
namespace Obss.Invoices.Tests.Domain;

public class InvoiceTests
{
    private static Invoice CreateDraftInvoice()
    {
        var tenantId = Guid.NewGuid().ToString("N");
        return Invoice.Create(
            tenantId, "INV-2026-00001", Guid.NewGuid(), "Test Customer",
            "test@example.com", "123 Test St",
            new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc), "USD");
    }

    private static InvoiceLine CreateLine(Guid invoiceId, LineType type, decimal total, decimal tax = 0, decimal taxRate = 0)
    {
        return new InvoiceLine(Guid.NewGuid(), invoiceId, Guid.NewGuid(), null,
            type, "Test line", 1, total, total, tax, taxRate, "USD");
    }

    [Fact]
    public void Create_ShouldSetInitialState()
    {
        var invoice = CreateDraftInvoice();

        invoice.Status.Should().Be(InvoiceStatus.Draft);
        invoice.SubTotal.Should().Be(0);
        invoice.GrandTotal.Should().Be(0);
        invoice.AmountDue.Should().Be(0);
        invoice.AmountPaid.Should().Be(0);
        invoice.Lines.Should().BeEmpty();
        invoice.Payments.Should().BeEmpty();
        invoice.InvoiceDate.Should().Be(new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc));
        invoice.DueDate.Should().Be(new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void AddLine_ShouldUpdateInvoiceTotals()
    {
        var invoice = CreateDraftInvoice();
        var line = CreateLine(invoice.Id, LineType.OneTime, 100m, 10m, 0.10m);

        invoice.AddLine(line);

        invoice.Lines.Should().HaveCount(1);
        invoice.SubTotal.Should().Be(100m);
        invoice.TaxTotal.Should().Be(10m);
        invoice.GrandTotal.Should().Be(110m);
        invoice.AmountDue.Should().Be(110m);
    }

    [Fact]
    public void AddLine_WithDiscountLine_ShouldReduceGrandTotal()
    {
        var invoice = CreateDraftInvoice();
        invoice.AddLine(CreateLine(invoice.Id, LineType.OneTime, 200m, 20m, 0.10m));
        invoice.AddLine(CreateLine(invoice.Id, LineType.Discount, -20m));

        invoice.SubTotal.Should().Be(180m);
        invoice.DiscountTotal.Should().Be(-20m);
        invoice.GrandTotal.Should().Be(180m);
    }

    [Fact]
    public void AddLine_WithTaxLineType_ShouldNotIncludeInSubTotal()
    {
        var invoice = CreateDraftInvoice();
        invoice.AddLine(CreateLine(invoice.Id, LineType.OneTime, 100m, 5m, 0.05m));
        invoice.AddLine(CreateLine(invoice.Id, LineType.Tax, 0m, 10m));

        invoice.SubTotal.Should().Be(100m);
        invoice.TaxTotal.Should().Be(15m);
    }

    [Fact]
    public void AddLines_ShouldCalculateMultipleLineTotals()
    {
        var invoice = CreateDraftInvoice();
        var line1 = CreateLine(invoice.Id, LineType.OneTime, 150m, 15m, 0.10m);
        var line2 = CreateLine(invoice.Id, LineType.Recurring, 99.99m, 10m, 0.10m);

        invoice.AddLines([line1, line2]);

        invoice.Lines.Should().HaveCount(2);
        invoice.SubTotal.Should().Be(249.99m);
        invoice.TaxTotal.Should().Be(25m);
        invoice.GrandTotal.Should().Be(274.99m);
    }

    [Fact]
    public void MarkAsFinalized_ShouldTransitionFromDraftToFinalized()
    {
        var invoice = CreateDraftInvoice();
        invoice.AddLine(CreateLine(invoice.Id, LineType.OneTime, 100m, 10m));

        invoice.MarkAsFinalized();

        invoice.Status.Should().Be(InvoiceStatus.Finalized);
    }

    [Fact]
    public void MarkAsFinalized_ShouldThrow_WhenNoLines()
    {
        var invoice = CreateDraftInvoice();

        var act = () => invoice.MarkAsFinalized();

        act.Should().Throw<InvalidInvoiceStateException>()
            .WithMessage("Cannot finalize an invoice with no lines.");
    }

    [Fact]
    public void MarkAsFinalized_ShouldThrow_WhenNotInDraft()
    {
        var invoice = CreateDraftInvoice();
        invoice.AddLine(CreateLine(invoice.Id, LineType.OneTime, 100m, 10m));
        invoice.MarkAsFinalized();

        var act = () => invoice.MarkAsFinalized();

        act.Should().Throw<InvalidInvoiceStateException>()
            .WithMessage("Cannot finalize invoice in 'Finalized' state. Only draft invoices can be finalized.");
    }

    [Fact]
    public void MarkAsSent_ShouldTransitionFromFinalizedToSent()
    {
        var invoice = CreateDraftInvoice();
        invoice.AddLine(CreateLine(invoice.Id, LineType.OneTime, 100m, 10m));
        invoice.MarkAsFinalized();

        invoice.MarkAsSent();

        invoice.Status.Should().Be(InvoiceStatus.Sent);
        invoice.SentAt.Should().NotBeNull();
    }

    [Fact]
    public void MarkAsSent_ShouldThrow_WhenNotFinalized()
    {
        var invoice = CreateDraftInvoice();

        var act = () => invoice.MarkAsSent();

        act.Should().Throw<InvalidInvoiceStateException>()
            .WithMessage("*must be finalized first*");
    }

    [Fact]
    public void RecordPayment_ShouldReduceAmountDue_OnSentInvoice()
    {
        var invoice = CreateDraftInvoice();
        invoice.AddLine(CreateLine(invoice.Id, LineType.OneTime, 200m, 20m));
        invoice.MarkAsFinalized();
        invoice.MarkAsSent();

        invoice.RecordPayment(50m, "PAY-001");

        invoice.AmountPaid.Should().Be(50m);
        invoice.AmountDue.Should().Be(170m);
        invoice.Payments.Should().HaveCount(1);
        invoice.Status.Should().Be(InvoiceStatus.Sent);
    }

    [Fact]
    public void RecordPayment_ShouldMarkAsPaid_WhenFullPayment()
    {
        var invoice = CreateDraftInvoice();
        invoice.AddLine(CreateLine(invoice.Id, LineType.OneTime, 200m, 20m));
        invoice.MarkAsFinalized();
        invoice.MarkAsSent();

        invoice.RecordPayment(220m, "PAY-002");

        invoice.Status.Should().Be(InvoiceStatus.Paid);
        invoice.AmountPaid.Should().Be(220m);
        invoice.AmountDue.Should().Be(0);
        invoice.PaidAt.Should().NotBeNull();
    }

    [Fact]
    public void RecordPayment_ShouldThrow_WhenAmountExceedsRemaining()
    {
        var invoice = CreateDraftInvoice();
        invoice.AddLine(CreateLine(invoice.Id, LineType.OneTime, 100m, 10m));
        invoice.MarkAsFinalized();
        invoice.MarkAsSent();

        var act = () => invoice.RecordPayment(200m, "PAY-003");

        act.Should().Throw<InvalidInvoiceStateException>()
            .WithMessage("*exceeds remaining balance*");
    }

    [Fact]
    public void RecordPayment_ShouldThrow_WhenInvoiceNotSentOrOverdue()
    {
        var invoice = CreateDraftInvoice();

        var act = () => invoice.RecordPayment(100m, "PAY-004");

        act.Should().Throw<InvalidInvoiceStateException>()
            .WithMessage("*Cannot record payment on invoice in 'Draft' state*");
    }

    [Fact]
    public void RecordPayment_ShouldWorkOnOverdueInvoice()
    {
        var invoice = CreateDraftInvoice();
        invoice.AddLine(CreateLine(invoice.Id, LineType.OneTime, 100m, 10m));
        invoice.MarkAsFinalized();
        invoice.MarkAsSent();
        invoice.MarkAsOverdue();

        invoice.RecordPayment(110m, "PAY-005");

        invoice.Status.Should().Be(InvoiceStatus.Paid);
    }

    [Fact]
    public void Cancel_ShouldSetStatusToCancelled_WhenInAllowedState()
    {
        var invoice = CreateDraftInvoice();

        invoice.Cancel("Customer request");

        invoice.Status.Should().Be(InvoiceStatus.Cancelled);
        invoice.Notes.Should().Be("Customer request");
    }

    [Fact]
    public void Cancel_ShouldThrow_WhenInvoiceIsPaid()
    {
        var invoice = CreateDraftInvoice();
        invoice.AddLine(CreateLine(invoice.Id, LineType.OneTime, 100m, 10m));
        invoice.MarkAsFinalized();
        invoice.MarkAsSent();
        invoice.RecordPayment(110m, "PAY-006");

        var act = () => invoice.Cancel("Too late");

        act.Should().Throw<InvalidInvoiceStateException>()
            .WithMessage("*Cannot cancel invoice in 'Paid' state*");
    }

    [Fact]
    public void Cancel_ShouldThrow_WhenInvoiceIsCancelled()
    {
        var invoice = CreateDraftInvoice();
        invoice.Cancel("First reason");

        var act = () => invoice.Cancel("Second reason");

        act.Should().Throw<InvalidInvoiceStateException>();
    }

    [Fact]
    public void MarkAsOverdue_ShouldTransitionFromSentToOverdue()
    {
        var invoice = CreateDraftInvoice();
        invoice.AddLine(CreateLine(invoice.Id, LineType.OneTime, 100m, 10m));
        invoice.MarkAsFinalized();
        invoice.MarkAsSent();

        invoice.MarkAsOverdue();

        invoice.Status.Should().Be(InvoiceStatus.Overdue);
    }

    [Fact]
    public void MarkAsOverdue_ShouldThrow_WhenNotSent()
    {
        var invoice = CreateDraftInvoice();

        var act = () => invoice.MarkAsOverdue();

        act.Should().Throw<InvalidInvoiceStateException>()
            .WithMessage("*Cannot mark invoice as overdue when in 'Draft' state*");
    }

    [Fact]
    public void IssueCreditNote_ShouldSucceed_WhenInvoiceIsSent()
    {
        var invoice = CreateDraftInvoice();
        invoice.AddLine(CreateLine(invoice.Id, LineType.OneTime, 500m, 50m));
        invoice.MarkAsFinalized();
        invoice.MarkAsSent();

        invoice.IssueCreditNote(100m, "Defective product");

        invoice.Status.Should().Be(InvoiceStatus.Sent);
    }

    [Fact]
    public void IssueCreditNote_ShouldThrow_WhenAmountExceedsGrandTotal()
    {
        var invoice = CreateDraftInvoice();
        invoice.AddLine(CreateLine(invoice.Id, LineType.OneTime, 100m, 10m));
        invoice.MarkAsFinalized();
        invoice.MarkAsSent();

        var act = () => invoice.IssueCreditNote(500m, "Overcharge");

        act.Should().Throw<InvalidInvoiceStateException>()
            .WithMessage("*Invalid credit note amount*");
    }

    [Fact]
    public void IssueCreditNote_ShouldThrow_WhenInvoiceIsDraft()
    {
        var invoice = CreateDraftInvoice();

        var act = () => invoice.IssueCreditNote(50m, "Not allowed");

        act.Should().Throw<InvalidInvoiceStateException>()
            .WithMessage("*Cannot issue credit note on invoice in 'Draft' state*");
    }

    [Fact]
    public void Void_ShouldSetStatusToVoid()
    {
        var invoice = CreateDraftInvoice();
        invoice.AddLine(CreateLine(invoice.Id, LineType.OneTime, 100m, 10m));
        invoice.MarkAsFinalized();

        invoice.Void();

        invoice.Status.Should().Be(InvoiceStatus.Void);
    }

    [Fact]
    public void Void_ShouldThrow_WhenAlreadyVoid()
    {
        var invoice = CreateDraftInvoice();
        invoice.Void();

        var act = () => invoice.Void();

        act.Should().Throw<InvalidInvoiceStateException>()
            .WithMessage("*Cannot void invoice in 'Void' state*");
    }

    [Fact]
    public void Void_ShouldThrow_WhenPaid()
    {
        var invoice = CreateDraftInvoice();
        invoice.AddLine(CreateLine(invoice.Id, LineType.OneTime, 100m, 10m));
        invoice.MarkAsFinalized();
        invoice.MarkAsSent();
        invoice.RecordPayment(110m, "PAY-007");

        var act = () => invoice.Void();

        act.Should().Throw<InvalidInvoiceStateException>();
    }

    [Fact]
    public void AddNote_ShouldAddNoteToCollection()
    {
        var invoice = CreateDraftInvoice();

        invoice.AddNote("Customer called about discount");

        invoice.NotesCollection.Should().HaveCount(1);
        invoice.NotesCollection.First().Content.Should().Be("Customer called about discount");
    }
}
