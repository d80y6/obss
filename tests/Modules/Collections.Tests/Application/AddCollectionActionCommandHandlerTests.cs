using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Obss.Collections.Application.Abstractions;
using Obss.Collections.Application.Commands.AddCollectionAction;
using Obss.Collections.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Collections.Tests.Application;

public class AddCollectionActionCommandHandlerTests
{
    private readonly ILogger<AddCollectionActionCommandHandler> _logger =
        Substitute.For<ILogger<AddCollectionActionCommandHandler>>();

    [Fact]
    public async Task Handle_ShouldAddActionAndReturnSuccess()
    {
        var caseRepository = Substitute.For<ICollectionCaseRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var @case = CollectionCase.Open("tenant-1", Guid.NewGuid(), "Test", 1000m, "USD");
        caseRepository.GetByIdWithDetailsAsync(@case.Id, Arg.Any<CancellationToken>())
            .Returns(@case);

        var handler = new AddCollectionActionCommandHandler(caseRepository, unitOfWork, _logger);
        var command = new AddCollectionActionCommand(
            @case.Id, "PhoneCall", 0, "Called customer", "agent-1", null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        @case.Actions.Should().ContainSingle();
        @case.Status.Should().Be(Obss.Collections.Domain.ValueObjects.CollectionCaseStatus.InProgress);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistingCase_ShouldReturnNotFound()
    {
        var caseRepository = Substitute.For<ICollectionCaseRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        caseRepository.GetByIdWithDetailsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((CollectionCase?)null);

        var handler = new AddCollectionActionCommandHandler(caseRepository, unitOfWork, _logger);
        var command = new AddCollectionActionCommand(
            Guid.NewGuid(), "Email", 0, "Test", "system", null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.NotFound");
    }
}
