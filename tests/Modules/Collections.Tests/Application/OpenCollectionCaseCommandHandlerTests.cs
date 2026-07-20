using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Obss.Collections.Application.Abstractions;
using Obss.Collections.Application.Commands.OpenCollectionCase;
using Obss.Collections.Application.DTOs;
using Obss.Collections.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Collections.Tests.Application;

public class OpenCollectionCaseCommandHandlerTests
{
    private readonly ICollectionCaseRepository _caseRepository = Substitute.For<ICollectionCaseRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentTenant _currentTenant = Substitute.For<ICurrentTenant>();
    private readonly ILogger<OpenCollectionCaseCommandHandler> _logger =
        Substitute.For<ILogger<OpenCollectionCaseCommandHandler>>();

    [Fact]
    public async Task Handle_ShouldOpenCaseAndReturnDto()
    {
        _currentTenant.TenantId.Returns("tenant-1");
        _caseRepository.GetByCustomerWithActiveArrangementAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((CollectionCase?)null);

        var handler = new OpenCollectionCaseCommandHandler(
            _caseRepository, _unitOfWork, _currentTenant, _logger);
        var command = new OpenCollectionCaseCommand(
            Guid.NewGuid(), "Test Customer", 1500m, "USD");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.CustomerName.Should().Be("Test Customer");
        result.Value.TotalOverdueAmount.Should().Be(1500m);
        result.Value.Currency.Should().Be("USD");
        result.Value.Status.Should().Be("Open");
        await _caseRepository.Received(1).AddAsync(Arg.Any<CollectionCase>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithoutTenant_ShouldReturnUnauthorized()
    {
        _currentTenant.TenantId.Returns(string.Empty);

        var handler = new OpenCollectionCaseCommandHandler(
            _caseRepository, _unitOfWork, _currentTenant, _logger);
        var command = new OpenCollectionCaseCommand(
            Guid.NewGuid(), "Test", 500m, "USD");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.Unauthorized");
        await _caseRepository.DidNotReceive().AddAsync(Arg.Any<CollectionCase>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithExistingActiveCase_ShouldReturnConflict()
    {
        _currentTenant.TenantId.Returns("tenant-1");
        var existingCase = CollectionCase.Open("tenant-1", Guid.NewGuid(), "Existing", 100m, "USD");
        _caseRepository.GetByCustomerWithActiveArrangementAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(existingCase);

        var handler = new OpenCollectionCaseCommandHandler(
            _caseRepository, _unitOfWork, _currentTenant, _logger);
        var command = new OpenCollectionCaseCommand(
            Guid.NewGuid(), "New Customer", 500m, "USD");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.Conflict");
    }
}
