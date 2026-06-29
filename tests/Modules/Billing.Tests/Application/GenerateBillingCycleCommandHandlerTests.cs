using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Obss.Billing.Application.Abstractions;
using Obss.Billing.Application.Commands.GenerateBillingCycle;
using Obss.Billing.Domain.Entities;
using Obss.Billing.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Tests;

public class GenerateBillingCycleCommandHandlerTests
{
    private readonly IBillingCycleRepository _billingCycleRepository = Substitute.For<IBillingCycleRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ILogger<GenerateBillingCycleCommandHandler> _logger = Substitute.For<ILogger<GenerateBillingCycleCommandHandler>>();
    private readonly GenerateBillingCycleCommandHandler _handler;

    public GenerateBillingCycleCommandHandlerTests()
    {
        _handler = new GenerateBillingCycleCommandHandler(_billingCycleRepository, _unitOfWork, _logger);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateCycle()
    {
        var customerId = Guid.NewGuid();
        _billingCycleRepository.GetByCustomerAsync(customerId, Arg.Any<CancellationToken>())
            .Returns((BillingCycle?)null);

        var command = new GenerateBillingCycleCommand(
            customerId, "Monthly",
            new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.CustomerId.Should().Be(customerId);
        result.Value.BillingPeriod.Should().Be("Monthly");
        await _billingCycleRepository.Received(1).AddAsync(Arg.Any<BillingCycle>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithExistingCycle_ShouldReturnConflict()
    {
        var customerId = Guid.NewGuid();
        var existingCycle = BillingCycle.Create(
            "tenant-1", customerId, BillingPeriod.Monthly,
            DateTime.UtcNow, DateTime.UtcNow.AddMonths(1));
        _billingCycleRepository.GetByCustomerAsync(customerId, Arg.Any<CancellationToken>())
            .Returns(existingCycle);

        var command = new GenerateBillingCycleCommand(
            customerId, "Monthly",
            DateTime.UtcNow, DateTime.UtcNow.AddMonths(1));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.Conflict");
    }

    [Fact]
    public async Task Handle_WithInvalidBillingPeriod_ShouldReturnFailure()
    {
        var command = new GenerateBillingCycleCommand(
            Guid.NewGuid(), "InvalidPeriod",
            DateTime.UtcNow, DateTime.UtcNow);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.Validation");
    }
}
