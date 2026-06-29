using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Obss.Billing.Application.Abstractions;
using Obss.Billing.Application.Commands.GenerateBill;
using Obss.Billing.Domain.Entities;
using Obss.Billing.Domain.Services;
using Obss.Billing.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Tests;

public class GenerateBillCommandHandlerTests
{
    private readonly IBillingCalculator _billingCalculator = Substitute.For<IBillingCalculator>();
    private readonly ITaxCalculator _taxCalculator = Substitute.For<ITaxCalculator>();
    private readonly IBillRepository _billRepository = Substitute.For<IBillRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ILogger<GenerateBillCommandHandler> _logger = Substitute.For<ILogger<GenerateBillCommandHandler>>();
    private readonly GenerateBillCommandHandler _handler;

    public GenerateBillCommandHandlerTests()
    {
        _handler = new GenerateBillCommandHandler(
            _billingCalculator, _taxCalculator, _billRepository, _unitOfWork, _logger);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateBill()
    {
        var customerId = Guid.NewGuid();
        var bill = CreateSampleBill(customerId);
        _billingCalculator.CalculateBill(customerId, Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(bill);
        _taxCalculator.CalculateTaxesAsync(bill, Arg.Any<CancellationToken>())
            .Returns(bill);

        var command = new GenerateBillCommand(
            customerId, "Customer A", "Monthly",
            DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow,
            DateTime.UtcNow.AddDays(15), "USD");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.CustomerId.Should().Be(customerId);
        await _billRepository.Received(1).AddAsync(bill, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidBillingPeriod_ShouldReturnFailure()
    {
        var command = new GenerateBillCommand(
            Guid.NewGuid(), "Customer", "InvalidPeriod",
            DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow, "USD");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.Validation");
    }

    private static Bill CreateSampleBill(Guid customerId)
    {
        var bill = Bill.Create(
            "tenant-1", customerId, "Customer A",
            BillingPeriod.Monthly,
            DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow,
            DateTime.UtcNow.AddDays(15), "USD");
        bill.AddLine(BillLine.CreateRecurring(
            bill.Id, "Service", Guid.NewGuid(), null, null,
            1, 100m, 0, 0.05m, "USD", DateTime.UtcNow));
        bill.CalculateTotals();
        return bill;
    }
}
