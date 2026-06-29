using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Obss.Invoices.Application.Abstractions;
using Obss.Invoices.Application.Commands.RecordInvoicePayment;
using Obss.Invoices.Domain.Entities;
using Obss.Invoices.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.Invoices.Tests.Application;

public class RecordInvoicePaymentCommandHandlerTests
{
    private static Invoice CreateSentInvoice()
    {
        var tenantId = TenantId.Create(Guid.NewGuid().ToString("N"));
        var invoice = Invoice.Create(tenantId, "INV-2026-00001", Guid.NewGuid(),
            "Test", "test@example.com", "Addr",
            DateTime.UtcNow, DateTime.UtcNow.AddDays(30), "USD");
        invoice.AddLine(new InvoiceLine(Guid.NewGuid(), invoice.Id, Guid.NewGuid(), null,
            LineType.OneTime, "Service", 1, 200m, 200m, 20m, 0.10m, "USD"));
        invoice.MarkAsFinalized();
        invoice.MarkAsSent();
        return invoice;
    }

    [Fact]
    public async Task Handle_ShouldRecordPayment_WhenValidInvoice()
    {
        var invoice = CreateSentInvoice();
        var repository = Substitute.For<IInvoiceRepository>();
        repository.GetByIdAsync(invoice.Id, Arg.Any<CancellationToken>()).Returns(invoice);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var logger = Substitute.For<ILogger<RecordInvoicePaymentCommandHandler>>();

        var handler = new RecordInvoicePaymentCommandHandler(repository, unitOfWork, logger);
        var command = new RecordInvoicePaymentCommand(invoice.Id, 100m, "PAY-REF-001");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        invoice.AmountPaid.Should().Be(100m);
        await repository.Received(1).UpdateAsync(invoice, Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldMarkAsPaid_WhenFullPayment()
    {
        var invoice = CreateSentInvoice();
        var repository = Substitute.For<IInvoiceRepository>();
        repository.GetByIdAsync(invoice.Id, Arg.Any<CancellationToken>()).Returns(invoice);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var logger = Substitute.For<ILogger<RecordInvoicePaymentCommandHandler>>();

        var handler = new RecordInvoicePaymentCommandHandler(repository, unitOfWork, logger);
        var command = new RecordInvoicePaymentCommand(invoice.Id, 220m, "PAY-FULL-001");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        invoice.Status.Should().Be(InvoiceStatus.Paid);
        invoice.AmountPaid.Should().Be(220m);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenInvoiceMissing()
    {
        var repository = Substitute.For<IInvoiceRepository>();
        repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Invoice?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var logger = Substitute.For<ILogger<RecordInvoicePaymentCommandHandler>>();

        var handler = new RecordInvoicePaymentCommandHandler(repository, unitOfWork, logger);
        var command = new RecordInvoicePaymentCommand(Guid.NewGuid(), 100m, "PAY-REF");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.NotFound");
    }
}
