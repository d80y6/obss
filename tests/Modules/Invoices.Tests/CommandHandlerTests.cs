using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Obss.Billing.Application.DTOs;
using Obss.Invoices.Application.Abstractions;
using Obss.Invoices.Application.Commands.CreateInvoiceFromBill;
using Obss.Invoices.Application.Commands.FinalizeInvoice;
using Obss.Invoices.Domain.Entities;
using Obss.Invoices.Domain.Exceptions;
using Obss.Invoices.Domain.ValueObjects;
using Obss.Invoices.Application.Commands.CancelInvoice;
using Obss.Invoices.Application.Commands.IssueCreditNote;
using Obss.Invoices.Application.Commands.MarkInvoiceAsSent;
using Obss.Invoices.Application.Commands.OpenDispute;
using Obss.Invoices.Application.Commands.RecordInvoicePayment;
using Obss.Invoices.Application.Commands.ResolveDispute;
using Obss.Invoices.Infrastructure.Persistence.Repositories;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.Invoices.Tests;

public class CommandHandlerTests : InvoiceIntegrationTests
{
    private static Result<BillDto> MakeBill(Guid billId, Guid customerId, string customerName, string currency)
    {
        return Result.Success(new BillDto(
            billId, "tenant-1", customerId, customerName, "Monthly",
            DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow,
            DateTime.UtcNow.AddDays(15), "Finalized",
            100, 0, 5, 105, currency,
            DateTime.UtcNow, DateTime.UtcNow, new List<BillLineDto>
            {
                new(Guid.NewGuid(), billId, "Recurring",
                    "Monthly subscription", null, null, null, 1, 100, 0, 5, 0, 100, "USD", DateTime.UtcNow, null)
            }));
    }

    [Fact]
    public async Task CreateInvoiceFromBillCommand_ShouldCreateInvoiceInDatabase()
    {
        using var context = CreateDbContext();
        var invoiceRepository = new InvoiceRepository(context);
        var billQuery = Substitute.For<IBillQuery>();
        var unitOfWork = CreateUnitOfWork(context);
        var logger = Substitute.For<ILogger<CreateInvoiceFromBillCommandHandler>>();

        var tenantId = Guid.NewGuid().ToString("N");
        var billId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        billQuery.GetBillByIdAsync(billId, Arg.Any<CancellationToken>())
            .Returns(MakeBill(billId, customerId, "Invoice Customer", "USD"));

        var handler = new CreateInvoiceFromBillCommandHandler(invoiceRepository, billQuery, unitOfWork, logger);

        var command = new CreateInvoiceFromBillCommand(
            tenantId,
            billId,
            customerId,
            "Invoice Customer",
            "invoice@example.com",
            "789 Invoice Blvd",
            "USD");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.InvoiceNumber.Should().NotBeNullOrEmpty();
        result.Value.CustomerName.Should().Be("Invoice Customer");
        result.Value.CustomerEmail.Should().Be("invoice@example.com");
        result.Value.CustomerAddress.Should().Be("789 Invoice Blvd");
        result.Value.Status.Should().Be("Draft");

        var saved = await invoiceRepository.GetByIdAsync(result.Value.Id);
        saved.Should().NotBeNull();
        saved!.InvoiceNumber.Should().Be(result.Value.InvoiceNumber);
    }

    [Fact]
    public async Task CreateInvoiceFromBillCommand_ShouldSetDueDateTo30Days()
    {
        using var context = CreateDbContext();
        var invoiceRepository = new InvoiceRepository(context);
        var billQuery = Substitute.For<IBillQuery>();
        var unitOfWork = CreateUnitOfWork(context);
        var logger = Substitute.For<ILogger<CreateInvoiceFromBillCommandHandler>>();

        var billId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        billQuery.GetBillByIdAsync(billId, Arg.Any<CancellationToken>())
            .Returns(MakeBill(billId, customerId, "Due Date Test", "USD"));

        var handler = new CreateInvoiceFromBillCommandHandler(invoiceRepository, billQuery, unitOfWork, logger);

        var command = new CreateInvoiceFromBillCommand(
            Guid.NewGuid().ToString("N"),
            billId,
            customerId,
            "Due Date Test",
            "due@example.com",
            "Addr",
            "USD");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Value.DueDate.Should().BeCloseTo(result.Value.InvoiceDate.AddDays(30), TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task FinalizeInvoiceCommand_ShouldFinalizeDraftInvoice()
    {
        using var context = CreateDbContext();
        var invoiceRepository = new InvoiceRepository(context);
        var unitOfWork = CreateUnitOfWork(context);
        var logger = Substitute.For<ILogger<FinalizeInvoiceCommandHandler>>();

        var tenantId = TenantId.Create(Guid.NewGuid().ToString("N"));
        var invoice = Invoice.Create(
            tenantId,
            "INV-2026-10001",
            Guid.NewGuid(),
            "Finalize Test",
            "finalize@example.com",
            "Addr",
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(30),
            "USD");

        var line = new InvoiceLine(
            Guid.NewGuid(),
            invoice.Id,
            Guid.NewGuid(),
            null,
            LineType.OneTime,
            "Service fee",
            1,
            200.00m,
            200.00m,
            20.00m,
            0.10m,
            "USD");
        invoice.AddLine(line);

        await invoiceRepository.AddAsync(invoice);
        await unitOfWork.SaveChangesAsync();

        var handler = new FinalizeInvoiceCommandHandler(invoiceRepository, unitOfWork, logger);
        var command = new FinalizeInvoiceCommand(invoice.Id);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var saved = await invoiceRepository.GetByIdWithDetailsAsync(invoice.Id);
        saved.Should().NotBeNull();
        saved!.Status.Should().Be(InvoiceStatus.Finalized);
    }

    [Fact]
    public async Task FinalizeInvoiceCommand_ForMissingInvoice_ShouldReturnNotFound()
    {
        using var context = CreateDbContext();
        var invoiceRepository = new InvoiceRepository(context);
        var unitOfWork = CreateUnitOfWork(context);
        var logger = Substitute.For<ILogger<FinalizeInvoiceCommandHandler>>();

        var handler = new FinalizeInvoiceCommandHandler(invoiceRepository, unitOfWork, logger);
        var command = new FinalizeInvoiceCommand(Guid.NewGuid());

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task FinalizeInvoiceCommand_ForInvoiceWithNoLines_ShouldThrowInvalidStateException()
    {
        using var context = CreateDbContext();
        var invoiceRepository = new InvoiceRepository(context);
        var unitOfWork = CreateUnitOfWork(context);
        var logger = Substitute.For<ILogger<FinalizeInvoiceCommandHandler>>();

        var tenantId = TenantId.Create(Guid.NewGuid().ToString("N"));
        var invoice = Invoice.Create(
            tenantId,
            "INV-2026-10002",
            Guid.NewGuid(),
            "Empty Invoice",
            "empty@example.com",
            "Addr",
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(30),
            "USD");

        await invoiceRepository.AddAsync(invoice);
        await unitOfWork.SaveChangesAsync();

        var handler = new FinalizeInvoiceCommandHandler(invoiceRepository, unitOfWork, logger);
        var command = new FinalizeInvoiceCommand(invoice.Id);

        await FluentActions
            .Awaiting(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidInvoiceStateException>()
            .WithMessage("Cannot finalize an invoice with no lines.");
    }

    [Fact]
    public async Task FinalizeInvoiceCommand_ForAlreadyFinalizedInvoice_ShouldThrowInvalidStateException()
    {
        using var context = CreateDbContext();
        var invoiceRepository = new InvoiceRepository(context);
        var unitOfWork = CreateUnitOfWork(context);
        var logger = Substitute.For<ILogger<FinalizeInvoiceCommandHandler>>();

        var tenantId = TenantId.Create(Guid.NewGuid().ToString("N"));
        var invoice = Invoice.Create(
            tenantId,
            "INV-2026-10003",
            Guid.NewGuid(),
            "Already Finalized",
            "finalized2@example.com",
            "Addr",
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(30),
            "USD");

        var line = new InvoiceLine(
            Guid.NewGuid(),
            invoice.Id,
            Guid.NewGuid(),
            null,
            LineType.OneTime,
            "Fee",
            1,
            100.00m,
            100.00m,
            10.00m,
            0.10m,
            "USD");
        invoice.AddLine(line);

        await invoiceRepository.AddAsync(invoice);
        await unitOfWork.SaveChangesAsync();

        var handler = new FinalizeInvoiceCommandHandler(invoiceRepository, unitOfWork, logger);

        var firstResult = await handler.Handle(new FinalizeInvoiceCommand(invoice.Id), CancellationToken.None);
        firstResult.IsSuccess.Should().BeTrue();

        await FluentActions
            .Awaiting(() => handler.Handle(new FinalizeInvoiceCommand(invoice.Id), CancellationToken.None))
            .Should().ThrowAsync<InvalidInvoiceStateException>()
            .WithMessage("Cannot finalize invoice in 'Finalized' state. Only draft invoices can be finalized.");
    }

    [Fact]
    public async Task CancelInvoiceCommand_ShouldCancelDraftInvoice()
    {
        using var context = CreateDbContext();
        var invoiceRepository = new InvoiceRepository(context);
        var unitOfWork = CreateUnitOfWork(context);
        var logger = Substitute.For<ILogger<CancelInvoiceCommandHandler>>();

        var tenantId = TenantId.Create(Guid.NewGuid().ToString("N"));
        var invoice = Invoice.Create(
            tenantId, "INV-2026-10010", Guid.NewGuid(), "Cancel Test",
            "cancel@example.com", "Addr",
            DateTime.UtcNow, DateTime.UtcNow.AddDays(30), "USD");

        await invoiceRepository.AddAsync(invoice);
        await unitOfWork.SaveChangesAsync();

        var handler = new CancelInvoiceCommandHandler(invoiceRepository, unitOfWork, logger);
        var command = new CancelInvoiceCommand(invoice.Id, "Customer request");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var saved = await invoiceRepository.GetByIdAsync(invoice.Id);
        saved.Should().NotBeNull();
        saved!.Status.Should().Be(InvoiceStatus.Cancelled);
        saved.Notes.Should().Be("Customer request");
    }

    [Fact]
    public async Task CancelInvoiceCommand_ForMissingInvoice_ShouldReturnNotFound()
    {
        using var context = CreateDbContext();
        var invoiceRepository = new InvoiceRepository(context);
        var unitOfWork = CreateUnitOfWork(context);
        var logger = Substitute.For<ILogger<CancelInvoiceCommandHandler>>();

        var handler = new CancelInvoiceCommandHandler(invoiceRepository, unitOfWork, logger);
        var command = new CancelInvoiceCommand(Guid.NewGuid(), "Reason");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task MarkInvoiceAsSentCommand_ShouldMarkFinalizedInvoiceAsSent()
    {
        using var context = CreateDbContext();
        var invoiceRepository = new InvoiceRepository(context);
        var unitOfWork = CreateUnitOfWork(context);
        var logger = Substitute.For<ILogger<MarkInvoiceAsSentCommandHandler>>();

        var tenantId = TenantId.Create(Guid.NewGuid().ToString("N"));
        var invoice = Invoice.Create(
            tenantId, "INV-2026-10011", Guid.NewGuid(), "Send Test",
            "send@example.com", "Addr",
            DateTime.UtcNow, DateTime.UtcNow.AddDays(30), "USD");

        var line = new InvoiceLine(
            Guid.NewGuid(), invoice.Id, Guid.NewGuid(), null,
            LineType.OneTime, "Fee", 1, 100m, 100m, 10m, 0.10m, "USD");
        invoice.AddLine(line);
        invoice.MarkAsFinalized();

        await invoiceRepository.AddAsync(invoice);
        await unitOfWork.SaveChangesAsync();

        var handler = new MarkInvoiceAsSentCommandHandler(invoiceRepository, unitOfWork, logger);
        var command = new MarkInvoiceAsSentCommand(invoice.Id);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var saved = await invoiceRepository.GetByIdWithDetailsAsync(invoice.Id);
        saved.Should().NotBeNull();
        saved!.Status.Should().Be(InvoiceStatus.Sent);
        saved.SentAt.Should().NotBeNull();
    }

    [Fact]
    public async Task RecordInvoicePaymentCommand_ShouldRecordPaymentAndMarkAsPaid()
    {
        using var context = CreateDbContext();
        var invoiceRepository = new InvoiceRepository(context);
        var unitOfWork = CreateUnitOfWork(context);
        var logger = Substitute.For<ILogger<RecordInvoicePaymentCommandHandler>>();

        var tenantId = TenantId.Create(Guid.NewGuid().ToString("N"));
        var invoice = Invoice.Create(
            tenantId, "INV-2026-10012", Guid.NewGuid(), "Payment Test",
            "pay@example.com", "Addr",
            DateTime.UtcNow, DateTime.UtcNow.AddDays(30), "USD");

        var line = new InvoiceLine(
            Guid.NewGuid(), invoice.Id, Guid.NewGuid(), null,
            LineType.OneTime, "Fee", 1, 200m, 200m, 20m, 0.10m, "USD");
        invoice.AddLine(line);
        invoice.MarkAsFinalized();
        invoice.MarkAsSent();

        await invoiceRepository.AddAsync(invoice);
        await unitOfWork.SaveChangesAsync();

        var handler = new RecordInvoicePaymentCommandHandler(invoiceRepository, unitOfWork, logger);
        var command = new RecordInvoicePaymentCommand(invoice.Id, 220m, "PAY-INT-001");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var saved = await invoiceRepository.GetByIdWithDetailsAsync(invoice.Id);
        saved.Should().NotBeNull();
        saved!.Status.Should().Be(InvoiceStatus.Paid);
        saved.AmountPaid.Should().Be(220m);
        saved.AmountDue.Should().Be(0);
        saved.PaidAt.Should().NotBeNull();
        saved.Payments.Should().HaveCount(1);
    }

    [Fact]
    public async Task OpenDisputeCommand_ShouldCreateDispute()
    {
        using var context = CreateDbContext();
        var invoiceRepository = new InvoiceRepository(context);
        var disputeRepository = new InvoiceDisputeRepository(context);
        var unitOfWork = CreateUnitOfWork(context);
        var logger = Substitute.For<ILogger<OpenDisputeCommandHandler>>();

        var tenantId = TenantId.Create(Guid.NewGuid().ToString("N"));
        var invoice = Invoice.Create(
            tenantId, "INV-2026-10013", Guid.NewGuid(), "Dispute Test",
            "dispute@example.com", "Addr",
            DateTime.UtcNow, DateTime.UtcNow.AddDays(30), "USD");

        await invoiceRepository.AddAsync(invoice);
        await unitOfWork.SaveChangesAsync();

        var handler = new OpenDisputeCommandHandler(invoiceRepository, disputeRepository, unitOfWork, logger);
        var command = new OpenDisputeCommand(invoice.Id, "Overcharge",
            "The invoice amount exceeds the agreed rate", 100m);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Reason.Should().Be("Overcharge");
        result.Value.Status.Should().Be("Open");

        var disputes = await disputeRepository.GetDisputesAsync(invoice.Id, null);
        disputes.Should().HaveCount(1);
    }

    [Fact]
    public async Task ResolveDisputeCommand_ShouldResolveOpenDispute()
    {
        using var context = CreateDbContext();
        var invoiceRepository = new InvoiceRepository(context);
        var disputeRepository = new InvoiceDisputeRepository(context);
        var unitOfWork = CreateUnitOfWork(context);
        var logger = Substitute.For<ILogger<ResolveDisputeCommandHandler>>();

        var invoice = Invoice.Create(
            TenantId.Create(Guid.NewGuid().ToString("N")),
            "INV-2026-10014", Guid.NewGuid(), "Resolve Test",
            "resolve@example.com", "Addr",
            DateTime.UtcNow, DateTime.UtcNow.AddDays(30), "USD");

        await invoiceRepository.AddAsync(invoice);
        await unitOfWork.SaveChangesAsync();

        var dispute = InvoiceDispute.Submit(invoice.Id, invoice.CustomerId,
            "Dispute", "Description", 100m);
        await disputeRepository.AddAsync(dispute);
        await unitOfWork.SaveChangesAsync();

        var handler = new ResolveDisputeCommandHandler(disputeRepository, unitOfWork, logger);
        var resolverId = Guid.NewGuid();
        var command = new ResolveDisputeCommand(dispute.Id, "Credit applied", resolverId);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var saved = await disputeRepository.GetByIdWithAttachmentsAsync(dispute.Id);
        saved.Should().NotBeNull();
        saved!.Status.Should().Be(DisputeStatus.Resolved);
        saved.Resolution.Should().Be("Credit applied");
        saved.ResolvedById.Should().Be(resolverId);
    }
}
