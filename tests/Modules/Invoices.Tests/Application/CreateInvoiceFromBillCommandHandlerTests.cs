using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Obss.Billing.Application.DTOs;
using Obss.Invoices.Application.Abstractions;
using Obss.Invoices.Application.Commands.CreateInvoiceFromBill;
using Obss.Invoices.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Invoices.Tests.Application;

public class CreateInvoiceFromBillCommandHandlerTests
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
    public async Task Handle_ShouldCreateInvoice_WhenValidCommand()
    {
        var repository = Substitute.For<IInvoiceRepository>();
        var billQuery = Substitute.For<IBillQuery>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var logger = Substitute.For<ILogger<CreateInvoiceFromBillCommandHandler>>();

        var billId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        billQuery.GetBillByIdAsync(billId, Arg.Any<CancellationToken>())
            .Returns(MakeBill(billId, customerId, "Test Customer", "USD"));

        repository.GenerateNextInvoiceNumberAsync(Arg.Any<CancellationToken>())
            .Returns("INV-2026-00001");

        var handler = new CreateInvoiceFromBillCommandHandler(repository, billQuery, unitOfWork, logger);
        var command = new CreateInvoiceFromBillCommand(
            Guid.NewGuid().ToString("N"), billId, customerId,
            "Test Customer", "test@example.com", "123 Test St", "USD");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.CustomerName.Should().Be("Test Customer");
        result.Value.CustomerEmail.Should().Be("test@example.com");
        result.Value.Status.Should().Be("Draft");
        result.Value.InvoiceNumber.Should().Be("INV-2026-00001");

        await repository.Received(1).AddAsync(Arg.Any<Invoice>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenBillMissing()
    {
        var repository = Substitute.For<IInvoiceRepository>();
        var billQuery = Substitute.For<IBillQuery>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var logger = Substitute.For<ILogger<CreateInvoiceFromBillCommandHandler>>();

        var billId = Guid.NewGuid();
        billQuery.GetBillByIdAsync(billId, Arg.Any<CancellationToken>())
            .Returns(Result.Failure<BillDto>(Error.NotFound("Bill", billId.ToString())));

        repository.GenerateNextInvoiceNumberAsync(Arg.Any<CancellationToken>())
            .Returns("INV-2026-00002");

        var handler = new CreateInvoiceFromBillCommandHandler(repository, billQuery, unitOfWork, logger);
        var command = new CreateInvoiceFromBillCommand(
            Guid.NewGuid().ToString("N"), billId, Guid.NewGuid(),
            "Test", "t@t.com", "Addr", "USD");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldCopyBillLines()
    {
        var repository = Substitute.For<IInvoiceRepository>();
        var billQuery = Substitute.For<IBillQuery>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var logger = Substitute.For<ILogger<CreateInvoiceFromBillCommandHandler>>();

        var billId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        billQuery.GetBillByIdAsync(billId, Arg.Any<CancellationToken>())
            .Returns(MakeBill(billId, customerId, "Line Test", "USD"));

        repository.GenerateNextInvoiceNumberAsync(Arg.Any<CancellationToken>())
            .Returns("INV-2026-00003");

        var handler = new CreateInvoiceFromBillCommandHandler(repository, billQuery, unitOfWork, logger);
        var command = new CreateInvoiceFromBillCommand(
            Guid.NewGuid().ToString("N"), billId, customerId,
            "Line Test", "line@example.com", "Addr", "USD");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Lines.Should().NotBeEmpty();
    }
}
