using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Obss.Invoices.Application.Abstractions;
using Obss.Invoices.Application.Commands.CancelInvoice;
using Obss.Invoices.Domain.Entities;
using Obss.Invoices.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
namespace Obss.Invoices.Tests.Application;

public class CancelInvoiceCommandHandlerTests
{
    private static Invoice CreateDraftInvoice()
    {
        var tenantId = Guid.NewGuid().ToString("N");
        return Invoice.Create(tenantId, "INV-2026-00001", Guid.NewGuid(),
            "Test", "test@example.com", "Addr",
            DateTime.UtcNow, DateTime.UtcNow.AddDays(30), "USD");
    }

    [Fact]
    public async Task Handle_ShouldCancelInvoice_WhenDraftInvoice()
    {
        var invoice = CreateDraftInvoice();
        var repository = Substitute.For<IInvoiceRepository>();
        repository.GetByIdAsync(invoice.Id, Arg.Any<CancellationToken>()).Returns(invoice);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var logger = Substitute.For<ILogger<CancelInvoiceCommandHandler>>();

        var handler = new CancelInvoiceCommandHandler(repository, unitOfWork, logger);
        var command = new CancelInvoiceCommand(invoice.Id, "Customer cancelled order");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        invoice.Status.Should().Be(InvoiceStatus.Cancelled);
        invoice.Notes.Should().Be("Customer cancelled order");
        await repository.Received(1).UpdateAsync(invoice, Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenInvoiceMissing()
    {
        var repository = Substitute.For<IInvoiceRepository>();
        repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Invoice?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var logger = Substitute.For<ILogger<CancelInvoiceCommandHandler>>();

        var handler = new CancelInvoiceCommandHandler(repository, unitOfWork, logger);
        var command = new CancelInvoiceCommand(Guid.NewGuid(), "Reason");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.NotFound");
    }
}
