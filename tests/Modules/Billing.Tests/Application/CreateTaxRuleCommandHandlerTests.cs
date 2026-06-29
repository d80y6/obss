using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Obss.Billing.Application.Abstractions;
using Obss.Billing.Application.Commands.CreateTaxRule;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Tests;

public class CreateTaxRuleCommandHandlerTests
{
    private readonly ITaxRuleRepository _taxRuleRepository = Substitute.For<ITaxRuleRepository>();
    private readonly ICurrentTenant _currentTenant = Substitute.For<ICurrentTenant>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ILogger<CreateTaxRuleCommandHandler> _logger = Substitute.For<ILogger<CreateTaxRuleCommandHandler>>();
    private readonly CreateTaxRuleCommandHandler _handler;

    public CreateTaxRuleCommandHandlerTests()
    {
        _currentTenant.TenantId.Returns("tenant-1");
        _handler = new CreateTaxRuleCommandHandler(_taxRuleRepository, _currentTenant, _unitOfWork, _logger);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateTaxRule()
    {
        var command = new CreateTaxRuleCommand(
            "VAT Standard", "Standard rate", 0.15m,
            "Percentage", "goods", "YE", "Sana'a",
            false, 1, DateTime.UtcNow.AddDays(-30), null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("VAT Standard");
        result.Value.TaxRate.Should().Be(0.15m);
        result.Value.TaxType.Should().Be("Percentage");
        result.Value.Country.Should().Be("YE");
        await _taxRuleRepository.Received(1).AddAsync(Arg.Any<Domain.Entities.TaxRule>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidTaxType_ShouldReturnFailure()
    {
        var command = new CreateTaxRuleCommand(
            "Bad Tax", "desc", 0.1m,
            "InvalidType", "goods", "YE", "",
            false, 1, DateTime.UtcNow, null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.Validation");
    }
}
