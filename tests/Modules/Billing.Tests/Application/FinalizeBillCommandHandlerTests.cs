using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Obss.Billing.Application.Abstractions;
using Obss.Billing.Application.Commands.FinalizeBill;
using Obss.Billing.Domain.Entities;
using Obss.Billing.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Tests;

public class FinalizeBillCommandHandlerTests
{
    private readonly IBillRepository _billRepository = Substitute.For<IBillRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ILogger<FinalizeBillCommandHandler> _logger = Substitute.For<ILogger<FinalizeBillCommandHandler>>();
    private readonly FinalizeBillCommandHandler _handler;

    public FinalizeBillCommandHandlerTests()
    {
        _handler = new FinalizeBillCommandHandler(_billRepository, _unitOfWork, _logger);
    }

    [Fact]
    public async Task Handle_WithExistingCalculatedBill_ShouldFinalize()
    {
        var bill = CreateCalculatedBill();
        _billRepository.GetByIdWithLinesAsync(bill.Id, Arg.Any<CancellationToken>())
            .Returns(bill);

        var result = await _handler.Handle(new FinalizeBillCommand(bill.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        bill.Status.Should().Be(BillStatus.Finalized);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistingBill_ShouldReturnNotFound()
    {
        _billRepository.GetByIdWithLinesAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Bill?)null);

        var result = await _handler.Handle(new FinalizeBillCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    private static Bill CreateCalculatedBill()
    {
        var bill = Bill.Create(
            "tenant-1", Guid.NewGuid(), "Customer",
            BillingPeriod.Monthly,
            DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow,
            DateTime.UtcNow.AddDays(15), "USD");
        bill.AddLine(BillLine.CreateRecurring(
            bill.Id, "Item", Guid.NewGuid(), null, null,
            1, 50m, 0, 0m, "USD", DateTime.UtcNow));
        bill.CalculateTotals();
        return bill;
    }
}
