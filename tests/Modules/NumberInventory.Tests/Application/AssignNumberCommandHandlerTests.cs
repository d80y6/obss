using Xunit;
using FluentAssertions;
using NSubstitute;
using Obss.NumberInventory.Application.Abstractions;
using Obss.NumberInventory.Application.Commands.AssignNumber;
using Obss.NumberInventory.Domain.Entities;
using Obss.NumberInventory.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.NumberInventory.Tests.Application;

public class AssignNumberCommandHandlerTests
{
    private readonly ITelephoneNumberRepository _numberRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AssignNumberCommandHandler _handler;

    public AssignNumberCommandHandlerTests()
    {
        _numberRepository = Substitute.For<ITelephoneNumberRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new AssignNumberCommandHandler(_numberRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ShouldAssignNumber_WhenNumberExistsAndIsAvailable()
    {
        var number = TelephoneNumber.Create("tenant-1", "+1234567890", NumberType.Mobile, 10.0m, "USD");
        var customerId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();
        _numberRepository.GetByIdAsync(number.Id, Arg.Any<CancellationToken>()).Returns(number);

        var result = await _handler.Handle(
            new AssignNumberCommand(number.Id, customerId, subscriptionId),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        number.Status.Should().Be(NumberStatus.Assigned);
        number.CustomerId.Should().Be(customerId);
        number.SubscriptionId.Should().Be(subscriptionId);
        await _numberRepository.Received(1).UpdateAsync(number, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNumberNotFound_ShouldReturnNotFound()
    {
        var numberId = Guid.NewGuid();
        _numberRepository.GetByIdAsync(numberId, Arg.Any<CancellationToken>()).Returns((TelephoneNumber?)null);

        var result = await _handler.Handle(
            new AssignNumberCommand(numberId, Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.NotFound");
        await _numberRepository.DidNotReceive().UpdateAsync(Arg.Any<TelephoneNumber>(), Arg.Any<CancellationToken>());
    }
}
