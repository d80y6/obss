using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Obss.Invoices.Application.Abstractions;
using Obss.Invoices.Application.Commands.IssueCreditNote;
using Obss.Invoices.Domain.Entities;
using Obss.Invoices.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
namespace Obss.Invoices.Tests.Application;

public class IssueCreditNoteCommandHandlerTests
{
    private static Invoice CreateSentInvoice()
    {
        var tenantId = Guid.NewGuid().ToString("N");
        var invoice = Invoice.Create(tenantId, "INV-2026-00001", Guid.NewGuid(),
            "Test", "test@example.com", "Addr",
            DateTime.UtcNow, DateTime.UtcNow.AddDays(30), "USD");
        invoice.AddLine(new InvoiceLine(Guid.NewGuid(), invoice.Id, Guid.NewGuid(), null,
            LineType.OneTime, "Service", 1, 500m, 500m, 50m, 0.10m, "USD"));
        invoice.MarkAsFinalized();
        invoice.MarkAsSent();
        return invoice;
    }

    [Fact]
    public async Task Handle_ShouldIssueCreditNote_WhenValidInvoice()
    {
        var invoice = CreateSentInvoice();
        var repository = Substitute.For<IInvoiceRepository>();
        repository.GetByIdAsync(invoice.Id, Arg.Any<CancellationToken>()).Returns(invoice);
        repository.AddCreditNoteAsync(Arg.Any<CreditNote>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(CreditNote.Create("test", "CN-001", Guid.NewGuid(), Guid.NewGuid(), "test", "USD")));
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var logger = Substitute.For<ILogger<IssueCreditNoteCommandHandler>>();

        var handler = new IssueCreditNoteCommandHandler(repository, unitOfWork, logger);
        var command = new IssueCreditNoteCommand(
            Guid.NewGuid().ToString("N"), invoice.Id, Guid.NewGuid(),
            "Defective product", "USD",
            [new CreditNoteLineInput(Guid.NewGuid(), "Refund for defective item", 100m, 1)]);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Reason.Should().Be("Defective product");
        result.Value.Status.Should().Be("Issued");
        await repository.Received(1).UpdateAsync(invoice, Arg.Any<CancellationToken>());
        await repository.Received(1).AddCreditNoteAsync(Arg.Any<CreditNote>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenInvoiceMissing()
    {
        var repository = Substitute.For<IInvoiceRepository>();
        repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Invoice?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var logger = Substitute.For<ILogger<IssueCreditNoteCommandHandler>>();

        var handler = new IssueCreditNoteCommandHandler(repository, unitOfWork, logger);
        var command = new IssueCreditNoteCommand(
            Guid.NewGuid().ToString("N"), Guid.NewGuid(), Guid.NewGuid(),
            "Reason", "USD",
            [new CreditNoteLineInput(Guid.NewGuid(), "Line", 50m, 1)]);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.NotFound");
    }
}
