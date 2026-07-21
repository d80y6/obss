using Xunit;
using FluentAssertions;
using Obss.Invoices.Domain.Entities;
using Obss.Invoices.Domain.Exceptions;
using Obss.Invoices.Domain.ValueObjects;
namespace Obss.Invoices.Tests.Domain;

public class CreditNoteTests
{
    private static CreditNote CreateDraftCreditNote()
    {
        var tenantId = Guid.NewGuid().ToString("N");
        return CreditNote.Create(
            tenantId, "CN-2026-00001", Guid.NewGuid(), Guid.NewGuid(),
            "Customer refund", "USD");
    }

    private static CreditNoteLine CreateLine(Guid creditNoteId, decimal amount)
    {
        return new CreditNoteLine(Guid.NewGuid(), creditNoteId, Guid.NewGuid(),
            "Credit line", amount, 1);
    }

    [Fact]
    public void Create_ShouldSetInitialState()
    {
        var creditNote = CreateDraftCreditNote();

        creditNote.Status.Should().Be(CreditNoteStatus.Draft);
        creditNote.SubTotal.Should().Be(0);
        creditNote.TotalAmount.Should().Be(0);
        creditNote.Lines.Should().BeEmpty();
        creditNote.Reason.Should().Be("Customer refund");
    }

    [Fact]
    public void AddLine_ShouldUpdateTotals()
    {
        var creditNote = CreateDraftCreditNote();
        var line = CreateLine(creditNote.Id, 250m);

        creditNote.AddLine(line);

        creditNote.Lines.Should().HaveCount(1);
        creditNote.SubTotal.Should().Be(250m);
        creditNote.TotalAmount.Should().Be(250m);
    }

    [Fact]
    public void AddLines_ShouldCalculateMultipleLineTotals()
    {
        var creditNote = CreateDraftCreditNote();
        var line1 = CreateLine(creditNote.Id, 100m);
        var line2 = CreateLine(creditNote.Id, 50m);

        creditNote.AddLines([line1, line2]);

        creditNote.Lines.Should().HaveCount(2);
        creditNote.SubTotal.Should().Be(150m);
        creditNote.TotalAmount.Should().Be(150m);
    }

    [Fact]
    public void Issue_ShouldTransitionFromDraftToIssued()
    {
        var creditNote = CreateDraftCreditNote();
        creditNote.AddLine(CreateLine(creditNote.Id, 100m));

        creditNote.Issue();

        creditNote.Status.Should().Be(CreditNoteStatus.Issued);
        creditNote.IssuedAt.Should().NotBe(default);
    }

    [Fact]
    public void Issue_ShouldThrow_WhenNoLines()
    {
        var creditNote = CreateDraftCreditNote();

        var act = () => creditNote.Issue();

        act.Should().Throw<InvalidInvoiceStateException>()
            .WithMessage("Cannot issue a credit note with no lines.");
    }

    [Fact]
    public void Issue_ShouldThrow_WhenAlreadyIssued()
    {
        var creditNote = CreateDraftCreditNote();
        creditNote.AddLine(CreateLine(creditNote.Id, 100m));
        creditNote.Issue();

        var act = () => creditNote.Issue();

        act.Should().Throw<InvalidInvoiceStateException>()
            .WithMessage("*Cannot issue credit note in 'Issued' state*");
    }

    [Fact]
    public void Apply_ShouldTransitionFromIssuedToApplied()
    {
        var creditNote = CreateDraftCreditNote();
        creditNote.AddLine(CreateLine(creditNote.Id, 100m));
        creditNote.Issue();

        creditNote.Apply();

        creditNote.Status.Should().Be(CreditNoteStatus.Applied);
        creditNote.AppliedAt.Should().NotBeNull();
    }

    [Fact]
    public void Apply_ShouldThrow_WhenNotIssued()
    {
        var creditNote = CreateDraftCreditNote();

        var act = () => creditNote.Apply();

        act.Should().Throw<InvalidInvoiceStateException>()
            .WithMessage("*Cannot apply credit note in 'Draft' state*");
    }

    [Fact]
    public void Void_ShouldTransitionToVoid()
    {
        var creditNote = CreateDraftCreditNote();
        creditNote.AddLine(CreateLine(creditNote.Id, 100m));
        creditNote.Issue();

        creditNote.Void();

        creditNote.Status.Should().Be(CreditNoteStatus.Void);
    }

    [Fact]
    public void Void_ShouldThrow_WhenAlreadyVoid()
    {
        var creditNote = CreateDraftCreditNote();
        creditNote.Void();

        var act = () => creditNote.Void();

        act.Should().Throw<InvalidInvoiceStateException>()
            .WithMessage("Credit note is already void.");
    }
}
