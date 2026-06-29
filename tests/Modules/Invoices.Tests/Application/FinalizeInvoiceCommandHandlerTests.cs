using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Obss.Invoices.Application.Abstractions;
using Obss.Invoices.Application.Commands.FinalizeInvoice;
using Obss.Invoices.Domain.Entities;
using Obss.Invoices.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.Invoices.Tests.Application;

public class FinalizeInvoiceCommandHandlerTests
{
    private static Invoice CreateDraftInvoiceWithLine()
    {
        var tenantId = TenantId.Create(Guid.NewGuid().ToString("N"));
        var invoice = Invoice.Create(tenantId, "INV-2026-00001", Guid.NewGuid(),
            "Test", "test@example.com", "Addr",
            DateTime.UtcNow, DateTime.UtcNow.AddDays(30), "USD");
        invoice.AddLine(new InvoiceLine(Guid.NewGuid(), invoice.Id, Guid.NewGuid(), null,
            LineType.OneTime, "Fee", 1, 100m, 100m, 10m, 0.10m, "USD"));
        return invoice;
    }

    [Fact]
    public async Task Handle_ShouldFinalizeInvoice_WhenDraftInvoiceWithLines()
    {
        var invoice = CreateDraftInvoiceWithLine();
        var repository = Substitute.For<IInvoiceRepository>();
        repository.GetByIdAsync(invoice.Id, Arg.Any<CancellationToken>()).Returns(invoice);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var logger = Substitute.For<ILogger<FinalizeInvoiceCommandHandler>>();

        var handler = new FinalizeInvoiceCommandHandler(repository, unitOfWork, logger);
        var command = new FinalizeInvoiceCommand(invoice.Id);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        invoice.Status.Should().Be(InvoiceStatus.Finalized);
        await repository.Received(1).UpdateAsync(invoice, Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenInvoiceMissing()
    {
        var repository = Substitute.For<IInvoiceRepository>();
        repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Invoice?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var logger = Substitute.For<ILogger<FinalizeInvoiceCommandHandler>>();

        var handler = new FinalizeInvoiceCommandHandler(repository, unitOfWork, logger);
        var command = new FinalizeInvoiceCommand(Guid.NewGuid());

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.NotFound");
    }
}
