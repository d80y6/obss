using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Obss.Billing.Application.Abstractions;
using Obss.Billing.Application.Commands.ApplyTaxExemption;
using Obss.Billing.Domain.Entities;
using Obss.Billing.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Tests;

public class ApplyTaxExemptionCommandHandlerTests
{
    private readonly ITaxRuleRepository _taxRuleRepository = Substitute.For<ITaxRuleRepository>();
    private readonly ICurrentTenant _currentTenant = Substitute.For<ICurrentTenant>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ILogger<ApplyTaxExemptionCommandHandler> _logger = Substitute.For<ILogger<ApplyTaxExemptionCommandHandler>>();
    private readonly ApplyTaxExemptionCommandHandler _handler;

    public ApplyTaxExemptionCommandHandlerTests()
    {
        _currentTenant.TenantId.Returns("tenant-1");
        _handler = new ApplyTaxExemptionCommandHandler(_taxRuleRepository, _currentTenant, _unitOfWork, _logger);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldCreateExemption()
    {
        var taxRuleId = Guid.NewGuid();
        var taxRule = TaxRule.Create(
            "tenant-1", "VAT", "desc", 0.15m,
            TaxType.Percentage, "goods", "YE", "", false, 1,
            DateTime.UtcNow.AddDays(-30), null);
        _taxRuleRepository.GetByIdAsync(taxRuleId, Arg.Any<CancellationToken>())
            .Returns(taxRule);

        var customerId = Guid.NewGuid();
        var command = new ApplyTaxExemptionCommand(
            customerId, taxRuleId, "CERT-100",
            0.5m, DateTime.UtcNow.AddDays(-30), DateTime.UtcNow.AddDays(30),
            "admin@test.com");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.CustomerId.Should().Be(customerId);
        result.Value.TaxRuleId.Should().Be(taxRuleId);
        result.Value.ExemptionRate.Should().Be(0.5m);
        await _taxRuleRepository.Received(1).AddExemptionAsync(Arg.Any<TaxExemption>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistingTaxRule_ShouldReturnNotFound()
    {
        _taxRuleRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((TaxRule?)null);

        var command = new ApplyTaxExemptionCommand(
            Guid.NewGuid(), Guid.NewGuid(), "CERT-100",
            0.5m, DateTime.UtcNow, DateTime.UtcNow.AddDays(30),
            "admin");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.NotFound");
    }
}
