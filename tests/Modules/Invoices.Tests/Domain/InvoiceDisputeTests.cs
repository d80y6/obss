using Xunit;
using FluentAssertions;
using Obss.Invoices.Domain.Entities;
using Obss.Invoices.Domain.ValueObjects;

namespace Obss.Invoices.Tests.Domain;

public class InvoiceDisputeTests
{
    [Fact]
    public void Submit_ShouldCreateDisputeWithOpenStatus()
    {
        var dispute = InvoiceDispute.Submit(
            Guid.NewGuid(), Guid.NewGuid(), "Incorrect charge",
            "The amount billed does not match the agreement", 150m);

        dispute.Status.Should().Be(DisputeStatus.Open);
        dispute.DisputedAmount.Should().Be(150m);
        dispute.Reason.Should().Be("Incorrect charge");
        dispute.Attachments.Should().BeEmpty();
        dispute.CreatedAt.Should().NotBe(default);
    }

    [Fact]
    public void AcceptResolution_ShouldTransitionFromOpenToResolved()
    {
        var dispute = InvoiceDispute.Submit(
            Guid.NewGuid(), Guid.NewGuid(), "Billing error",
            "Amount is wrong", 200m);

        dispute.AcceptResolution("Credit issued", Guid.NewGuid());

        dispute.Status.Should().Be(DisputeStatus.Resolved);
        dispute.Resolution.Should().Be("Credit issued");
        dispute.ResolvedById.Should().NotBeNull();
        dispute.ResolvedAt.Should().NotBeNull();
    }

    [Fact]
    public void AcceptResolution_ShouldThrow_WhenAlreadyResolved()
    {
        var dispute = InvoiceDispute.Submit(
            Guid.NewGuid(), Guid.NewGuid(), "Test", "Desc", 100m);
        dispute.AcceptResolution("Resolved", Guid.NewGuid());

        var act = () => dispute.AcceptResolution("Again", Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RejectResolution_ShouldTransitionFromOpenToRejected()
    {
        var dispute = InvoiceDispute.Submit(
            Guid.NewGuid(), Guid.NewGuid(), "Dispute", "Description", 100m);

        dispute.RejectResolution("Customer claim is invalid");

        dispute.Status.Should().Be(DisputeStatus.Rejected);
        dispute.Resolution.Should().Be("Customer claim is invalid");
        dispute.ResolvedAt.Should().NotBeNull();
    }

    [Fact]
    public void RejectResolution_ShouldThrow_WhenAlreadyRejected()
    {
        var dispute = InvoiceDispute.Submit(
            Guid.NewGuid(), Guid.NewGuid(), "Test", "Desc", 100m);
        dispute.RejectResolution("No");

        var act = () => dispute.RejectResolution("Still no");

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AddAttachment_ShouldAddToCollection()
    {
        var dispute = InvoiceDispute.Submit(
            Guid.NewGuid(), Guid.NewGuid(), "Test", "Desc", 100m);

        dispute.AddAttachment("invoice.pdf", "application/pdf", 1024, "/uploads/invoice.pdf");

        dispute.Attachments.Should().HaveCount(1);
        var attachment = dispute.Attachments.First();
        attachment.FileName.Should().Be("invoice.pdf");
        attachment.ContentType.Should().Be("application/pdf");
        attachment.FileSize.Should().Be(1024);
    }

    [Fact]
    public void AddAttachment_ShouldSupportMultipleAttachments()
    {
        var dispute = InvoiceDispute.Submit(
            Guid.NewGuid(), Guid.NewGuid(), "Test", "Desc", 100m);

        dispute.AddAttachment("doc1.pdf", "application/pdf", 100, "/uploads/doc1.pdf");
        dispute.AddAttachment("doc2.jpg", "image/jpeg", 200, "/uploads/doc2.jpg");
        dispute.AddAttachment("doc3.png", "image/png", 300, "/uploads/doc3.png");

        dispute.Attachments.Should().HaveCount(3);
    }

    [Fact]
    public void Submit_ShouldSetCorrectProperties()
    {
        var invoiceId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var dispute = InvoiceDispute.Submit(
            invoiceId, customerId, "Overcharge",
            "Billed amount exceeds agreed rate", 500m);

        dispute.InvoiceId.Should().Be(invoiceId);
        dispute.CustomerId.Should().Be(customerId);
        dispute.Description.Should().Be("Billed amount exceeds agreed rate");
        dispute.DisputedAmount.Should().Be(500m);
    }
}
