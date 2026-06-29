using Xunit;
using FluentAssertions;
using NSubstitute;
using Obss.IAM.Application.Commands.UpdateUser;
using Obss.IAM.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.IAM.Tests.Application;

public class UpdateUserCommandHandlerTests
{
    private readonly IRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UpdateUserCommandHandler _handler;

    public UpdateUserCommandHandlerTests()
    {
        _userRepository = Substitute.For<IRepository<User>>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new UpdateUserCommandHandler(_userRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WithExistingUser_ShouldUpdateProfile()
    {
        var userId = Guid.NewGuid();
        var command = new UpdateUserCommand(userId, "NewFirst", "NewLast", null, null);

        var user = User.Create(
            SharedKernel.Domain.ValueObjects.TenantId.Create(Guid.NewGuid().ToString("N")),
            "testuser",
            SharedKernel.Domain.ValueObjects.Email.Create("test@example.com"),
            "OldFirst", "OldLast");

        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.FirstName.Should().Be("NewFirst");
        result.Value.LastName.Should().Be("NewLast");
        await _userRepository.Received(1).UpdateAsync(user, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistingUser_ShouldReturnNotFound()
    {
        var userId = Guid.NewGuid();
        var command = new UpdateUserCommand(userId, "NewFirst", "NewLast", null, null);

        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.NotFound");
        await _userRepository.DidNotReceive().UpdateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithPhoneNumber_ShouldUpdateWithPhone()
    {
        var userId = Guid.NewGuid();
        var command = new UpdateUserCommand(userId, "Test", "User", "712345678", "+967");

        var user = User.Create(
            SharedKernel.Domain.ValueObjects.TenantId.Create(Guid.NewGuid().ToString("N")),
            "testuser",
            SharedKernel.Domain.ValueObjects.Email.Create("test@example.com"),
            "OldFirst", "OldLast");

        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.PhoneNumber.Should().Be("+967712345678");
    }

    [Fact]
    public async Task Handle_WithEmptyPhone_ShouldClearPhone()
    {
        var userId = Guid.NewGuid();
        var command = new UpdateUserCommand(userId, "Test", "User", "", null);

        var user = User.Create(
            SharedKernel.Domain.ValueObjects.TenantId.Create(Guid.NewGuid().ToString("N")),
            "testuser",
            SharedKernel.Domain.ValueObjects.Email.Create("test@example.com"),
            "OldFirst", "OldLast");
        user.UpdateProfile("OldFirst", "OldLast", PhoneNumber.Create("712345678"));

        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.PhoneNumber.Should().BeNull();
    }
}
