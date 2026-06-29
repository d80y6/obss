using Xunit;
using FluentAssertions;
using NSubstitute;
using Obss.Rating.Application.Abstractions;
using Obss.Rating.Application.Commands.CreatePromotion;
using Obss.Rating.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Rating.Tests.Application;

public class CreatePromotionCommandHandlerTests
{
    private readonly IPromotionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenant _currentTenant;
    private readonly CreatePromotionCommandHandler _handler;

    public CreatePromotionCommandHandlerTests()
    {
        _repository = Substitute.For<IPromotionRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _currentTenant = Substitute.For<ICurrentTenant>();
        _currentTenant.TenantId.Returns("tenant-1");
        _handler = new CreatePromotionCommandHandler(_repository, _unitOfWork, _currentTenant);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreatePromotion()
    {
        var command = new CreatePromotionCommand(
            "Summer Sale", "20% off", "Percentage", 20m, "USD",
            null, null, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(30),
            true, 1, "SUMMER20", 1000, []);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Summer Sale");
        result.Value.PromotionType.Should().Be("Percentage");
        result.Value.Code.Should().Be("SUMMER20");
        await _repository.Received(1).AddAsync(Arg.Any<Promotion>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidPromotionType_ShouldReturnFailure()
    {
        var command = new CreatePromotionCommand(
            "Bad", null, "UnknownType", 10m, "USD",
            null, null, DateTime.UtcNow, null,
            true, 1, null, null, []);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    [Fact]
    public async Task Handle_WithoutTenant_ShouldReturnUnauthorized()
    {
        _currentTenant.TenantId.Returns((string?)null);
        var command = new CreatePromotionCommand(
            "Test", null, "Percentage", 10m, "USD",
            null, null, DateTime.UtcNow, null,
            true, 1, null, null, []);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Unauthorized");
    }

    [Fact]
    public async Task Handle_WithRules_ShouldAddRules()
    {
        var command = new CreatePromotionCommand(
            "Targeted", null, "FixedAmount", 50m, "USD",
            1, 10, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(30),
            false, 2, null, null,
        [
            new CreatePromotionRuleDto("CustomerType", "Equals", "Enterprise", "And"),
        ]);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }
}
