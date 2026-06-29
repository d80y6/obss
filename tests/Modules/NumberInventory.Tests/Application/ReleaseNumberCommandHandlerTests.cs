using Xunit;
using FluentAssertions;
using NSubstitute;
using Obss.NumberInventory.Application.Abstractions;
using Obss.NumberInventory.Application.Commands.ReleaseNumber;
using Obss.NumberInventory.Domain.Entities;
using Obss.NumberInventory.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.NumberInventory.Tests.Application;

public class ReleaseNumberCommandHandlerTests
{
    private readonly ITelephoneNumberRepository _numberRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ReleaseNumberCommandHandler _handler;

    public ReleaseNumberCommandHandlerTests()
    {
        _numberRepository = Substitute.For<ITelephoneNumberRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new ReleaseNumberCommandHandler(_numberRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ShouldReleaseNumber_WhenNumberExistsAndIsAssigned()
    {
        var number = TelephoneNumber.Create("tenant-1", "+1234567890", NumberType.Mobile, 10.0m, "USD");
        number.Assign(Guid.NewGuid(), Guid.NewGuid());
        _numberRepository.GetByIdAsync(number.Id, Arg.Any<CancellationToken>()).Returns(number);

        var result = await _handler.Handle(
            new ReleaseNumberCommand(number.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        number.Status.Should().Be(NumberStatus.Available);
        number.CustomerId.Should().BeNull();
        number.SubscriptionId.Should().BeNull();
        await _numberRepository.Received(1).UpdateAsync(number, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNumberNotFound_ShouldReturnNotFound()
    {
        var numberId = Guid.NewGuid();
        _numberRepository.GetByIdAsync(numberId, Arg.Any<CancellationToken>()).Returns((TelephoneNumber?)null);

        var result = await _handler.Handle(
            new ReleaseNumberCommand(numberId),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.NotFound");
    }
}
