using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Obss.Billing.Application.Abstractions;
using Obss.Billing.Application.Commands.AddAdjustment;
using Obss.Billing.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Tests;

public class AddAdjustmentCommandHandlerTests
{
    private readonly IBillRepository _billRepository = Substitute.For<IBillRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ILogger<AddAdjustmentCommandHandler> _logger = Substitute.For<ILogger<AddAdjustmentCommandHandler>>();
    private readonly AddAdjustmentCommandHandler _handler;

    public AddAdjustmentCommandHandlerTests()
    {
        _handler = new AddAdjustmentCommandHandler(_billRepository, _unitOfWork, _logger);
    }

    [Fact]
    public async Task Handle_WithExistingDraftBill_ShouldAddAdjustment()
    {
        var bill = CreateDraftBill();
        _billRepository.GetByIdWithLinesAsync(bill.Id, Arg.Any<CancellationToken>())
            .Returns(bill);

        var command = new AddAdjustmentCommand(bill.Id, "Credit discount", -25m, "USD");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        bill.Lines.Should().ContainSingle(l => l.LineType == Domain.ValueObjects.LineType.Adjustment);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistingBill_ShouldReturnNotFound()
    {
        _billRepository.GetByIdWithLinesAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Bill?)null);

        var result = await _handler.Handle(
            new AddAdjustmentCommand(Guid.NewGuid(), "Test", 10m, "USD"),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    private static Bill CreateDraftBill()
    {
        return Bill.Create(
            "tenant-1", Guid.NewGuid(), "Customer",
            Domain.ValueObjects.BillingPeriod.Monthly,
            DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow,
            DateTime.UtcNow.AddDays(15), "USD");
    }
}
