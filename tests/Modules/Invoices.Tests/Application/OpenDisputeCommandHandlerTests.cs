using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Obss.Invoices.Application.Abstractions;
using Obss.Invoices.Application.Commands.OpenDispute;
using Obss.Invoices.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.Invoices.Tests.Application;

public class OpenDisputeCommandHandlerTests
{
    private static Invoice CreateInvoice()
    {
        var tenantId = TenantId.Create(Guid.NewGuid().ToString("N"));
        return Invoice.Create(tenantId, "INV-2026-00001", Guid.NewGuid(),
            "Test", "test@example.com", "Addr",
            DateTime.UtcNow, DateTime.UtcNow.AddDays(30), "USD");
    }

    [Fact]
    public async Task Handle_ShouldOpenDispute_WhenValidInvoice()
    {
        var invoice = CreateInvoice();
        var invoiceRepository = Substitute.For<IInvoiceRepository>();
        invoiceRepository.GetByIdAsync(invoice.Id, Arg.Any<CancellationToken>()).Returns(invoice);
        var disputeRepository = Substitute.For<IInvoiceDisputeRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var logger = Substitute.For<ILogger<OpenDisputeCommandHandler>>();

        var handler = new OpenDisputeCommandHandler(invoiceRepository, disputeRepository, unitOfWork, logger);
        var command = new OpenDisputeCommand(invoice.Id, "Incorrect charge",
            "Amount billed does not match contract", 150m);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Reason.Should().Be("Incorrect charge");
        result.Value.DisputedAmount.Should().Be(150m);
        result.Value.Status.Should().Be("Open");

        await disputeRepository.Received(1).AddAsync(Arg.Any<InvoiceDispute>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenInvoiceMissing()
    {
        var invoiceRepository = Substitute.For<IInvoiceRepository>();
        invoiceRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Invoice?)null);
        var disputeRepository = Substitute.For<IInvoiceDisputeRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var logger = Substitute.For<ILogger<OpenDisputeCommandHandler>>();

        var handler = new OpenDisputeCommandHandler(invoiceRepository, disputeRepository, unitOfWork, logger);
        var command = new OpenDisputeCommand(Guid.NewGuid(), "Reason", "Description", 100m);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.NotFound");
    }
}
