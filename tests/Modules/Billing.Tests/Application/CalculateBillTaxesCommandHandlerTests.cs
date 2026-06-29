using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Obss.Billing.Application.Abstractions;
using Obss.Billing.Application.Commands.CalculateBillTaxes;
using Obss.Billing.Domain.Entities;
using Obss.Billing.Domain.Services;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Tests;

public class CalculateBillTaxesCommandHandlerTests
{
    private readonly IBillRepository _billRepository = Substitute.For<IBillRepository>();
    private readonly ITaxCalculator _taxCalculator = Substitute.For<ITaxCalculator>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ILogger<CalculateBillTaxesCommandHandler> _logger = Substitute.For<ILogger<CalculateBillTaxesCommandHandler>>();
    private readonly CalculateBillTaxesCommandHandler _handler;

    public CalculateBillTaxesCommandHandlerTests()
    {
        _handler = new CalculateBillTaxesCommandHandler(_billRepository, _taxCalculator, _unitOfWork, _logger);
    }

    [Fact]
    public async Task Handle_WithExistingBill_ShouldCalculateTaxes()
    {
        var bill = CreateDraftBill();
        _billRepository.GetByIdWithLinesAsync(bill.Id, Arg.Any<CancellationToken>())
            .Returns(bill);
        _taxCalculator.CalculateTaxesAsync(bill, Arg.Any<CancellationToken>())
            .Returns(bill);

        var command = new CalculateBillTaxesCommand(bill.Id);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(bill.Id);
        await _taxCalculator.Received(1).CalculateTaxesAsync(bill, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistingBill_ShouldReturnNotFound()
    {
        _billRepository.GetByIdWithLinesAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Bill?)null);

        var result = await _handler.Handle(
            new CalculateBillTaxesCommand(Guid.NewGuid()),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    private static Bill CreateDraftBill()
    {
        var bill = Bill.Create(
            "tenant-1", Guid.NewGuid(), "Customer",
            Domain.ValueObjects.BillingPeriod.Monthly,
            DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow,
            DateTime.UtcNow.AddDays(15), "USD");
        bill.AddLine(BillLine.CreateRecurring(
            bill.Id, "Item", Guid.NewGuid(), null, null,
            1, 100m, 0, 0.05m, "USD", DateTime.UtcNow));
        return bill;
    }
}
