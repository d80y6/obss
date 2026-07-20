using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Obss.Collections.Application.Abstractions;
using Obss.Collections.Application.Commands.CreatePaymentArrangement;
using Obss.Collections.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Collections.Tests.Application;

public class CreatePaymentArrangementCommandHandlerTests
{
    private readonly ILogger<CreatePaymentArrangementCommandHandler> _logger =
        Substitute.For<ILogger<CreatePaymentArrangementCommandHandler>>();

    [Fact]
    public async Task Handle_ShouldCreateArrangementAndReturnDto()
    {
        var caseRepository = Substitute.For<ICollectionCaseRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var @case = CollectionCase.Open("tenant-1", Guid.NewGuid(), "Test", 2000m, "USD");
        caseRepository.GetByIdWithDetailsAsync(@case.Id, Arg.Any<CancellationToken>())
            .Returns(@case);

        var handler = new CreatePaymentArrangementCommandHandler(caseRepository, unitOfWork, _logger);
        var command = new CreatePaymentArrangementCommand(
            @case.Id, 1200m, 6, 200m, "Monthly", DateTime.UtcNow.AddDays(7));

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.PaymentArrangements.Should().ContainSingle();
        var arrangement = result.Value.PaymentArrangements.Single();
        arrangement.TotalAmount.Should().Be(1200m);
        arrangement.InstallmentCount.Should().Be(6);
        arrangement.Status.Should().Be("Active");
        @case.PaymentArrangements.Should().ContainSingle();
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistingCase_ShouldReturnNotFound()
    {
        var caseRepository = Substitute.For<ICollectionCaseRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        caseRepository.GetByIdWithDetailsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((CollectionCase?)null);

        var handler = new CreatePaymentArrangementCommandHandler(caseRepository, unitOfWork, _logger);
        var command = new CreatePaymentArrangementCommand(
            Guid.NewGuid(), 500m, 5, 100m, "Weekly", DateTime.UtcNow.AddDays(7));

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.NotFound");
    }
}
