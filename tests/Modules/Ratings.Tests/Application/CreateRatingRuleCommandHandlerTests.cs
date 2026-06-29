using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Obss.Rating.Application.Abstractions;
using Obss.Rating.Application.Commands.CreateRatingRule;
using Obss.Rating.Application.DTOs;
using Obss.Rating.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Rating.Tests.Application;

public class CreateRatingRuleCommandHandlerTests
{
    private readonly IRatingRuleRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenant _currentTenant;
    private readonly CreateRatingRuleCommandHandler _handler;

    public CreateRatingRuleCommandHandlerTests()
    {
        _repository = Substitute.For<IRatingRuleRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _currentTenant = Substitute.For<ICurrentTenant>();
        _currentTenant.TenantId.Returns("tenant-1");
        _handler = new CreateRatingRuleCommandHandler(_repository, _unitOfWork, _currentTenant);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateRule()
    {
        var command = new CreateRatingRuleCommand(
            "Voice Rate", "Per-minute voice rate", "Usage",
            null, null, 1,
            [new CreateRatingTierDto(0, null, 0.05m, "per minute")]);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Voice Rate");
        result.Value.RuleType.Should().Be("Usage");
        await _repository.Received(1).AddAsync(Arg.Any<RatingRule>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidRuleType_ShouldReturnFailure()
    {
        var command = new CreateRatingRuleCommand(
            "Bad Rule", null, "InvalidType",
            null, null, 1, []);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    [Fact]
    public async Task Handle_WithoutTenant_ShouldReturnUnauthorized()
    {
        _currentTenant.TenantId.Returns((string?)null);
        var command = new CreateRatingRuleCommand(
            "Test", null, "Flat", null, null, 1, []);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Unauthorized");
    }

    [Fact]
    public async Task Handle_WithTiers_ShouldAddTiersToRule()
    {
        RatingRule? capturedRule = null;
        await _repository.AddAsync(Arg.Do<RatingRule>(r => capturedRule = r), Arg.Any<CancellationToken>());

        var command = new CreateRatingRuleCommand(
            "Tiered Data", null, "Volume", null, null, 1,
        [
            new CreateRatingTierDto(0, 1024, 0.10m, "first GB"),
            new CreateRatingTierDto(1024, null, 0.05m, "after first GB"),
        ]);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        capturedRule.Should().NotBeNull();
    }
}
