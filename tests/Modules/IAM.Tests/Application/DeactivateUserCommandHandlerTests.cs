using Xunit;
using FluentAssertions;
using NSubstitute;
using Obss.IAM.Application.Commands.DeactivateUser;
using Obss.IAM.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.IAM.Tests.Application;

public class DeactivateUserCommandHandlerTests
{
    private readonly IRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly DeactivateUserCommandHandler _handler;

    public DeactivateUserCommandHandlerTests()
    {
        _userRepository = Substitute.For<IRepository<User>>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new DeactivateUserCommandHandler(_userRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WithExistingUser_ShouldDeactivate()
    {
        var userId = Guid.NewGuid();
        var user = User.Create(
            SharedKernel.Domain.ValueObjects.TenantId.Create(Guid.NewGuid().ToString("N")),
            "testuser",
            SharedKernel.Domain.ValueObjects.Email.Create("test@example.com"),
            "Test", "User");
        var command = new DeactivateUserCommand(userId);

        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        user.IsActive.Should().BeFalse();
        await _userRepository.Received(1).UpdateAsync(user, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistingUser_ShouldReturnNotFound()
    {
        var userId = Guid.NewGuid();
        var command = new DeactivateUserCommand(userId);

        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.NotFound");
        await _userRepository.DidNotReceive().UpdateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserAlreadyInactive_ShouldStillSucceed()
    {
        var userId = Guid.NewGuid();
        var user = User.Create(
            SharedKernel.Domain.ValueObjects.TenantId.Create(Guid.NewGuid().ToString("N")),
            "testuser",
            SharedKernel.Domain.ValueObjects.Email.Create("test@example.com"),
            "Test", "User");
        user.Deactivate();
        var command = new DeactivateUserCommand(userId);

        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }
}
