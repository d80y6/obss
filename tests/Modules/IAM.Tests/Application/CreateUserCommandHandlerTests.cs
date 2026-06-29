using Xunit;
using FluentAssertions;
using NSubstitute;
using Obss.IAM.Application.Abstractions;
using Obss.IAM.Application.Commands.CreateUser;
using Obss.IAM.Domain.Entities;
using Obss.IAM.Domain.Events;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.IAM.Tests.Application;

public class CreateUserCommandHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreateUserCommandHandler _handler;

    public CreateUserCommandHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new CreateUserCommandHandler(_userRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateUser()
    {
        var tenantId = Guid.NewGuid().ToString("N");
        var command = new CreateUserCommand(tenantId, "johndoe", "john@example.com", "John", "Doe", null, null, null);

        _userRepository.GetByEmailAsync("john@example.com", Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _userRepository.GetByUsernameAsync("johndoe", tenantId, Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _userRepository.When(x => x.AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>()))
            .Do(callInfo =>
            {
                var user = callInfo.Arg<User>();
                user.Should().NotBeNull();
                user.Username.Should().Be("johndoe");
            });
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Username.Should().Be("johndoe");
        result.Value.Email.Should().Be("john@example.com");
        await _userRepository.Received(1).AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithDuplicateEmail_ShouldReturnFailure()
    {
        var tenantId = Guid.NewGuid().ToString("N");
        var command = new CreateUserCommand(tenantId, "johndoe", "existing@example.com", "John", "Doe", null, null, null);
        var existingUser = User.Create(
            SharedKernel.Domain.ValueObjects.TenantId.Create(tenantId),
            "existing",
            SharedKernel.Domain.ValueObjects.Email.Create("existing@example.com"),
            "Existing", "User");

        _userRepository.GetByEmailAsync("existing@example.com", Arg.Any<CancellationToken>())
            .Returns(existingUser);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.Conflict");
        result.Error.Description.Should().Contain("already registered");
        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithDuplicateUsername_ShouldReturnFailure()
    {
        var tenantId = Guid.NewGuid().ToString("N");
        var command = new CreateUserCommand(tenantId, "dupeuser", "unique@example.com", "John", "Doe", null, null, null);
        var existingUser = User.Create(
            SharedKernel.Domain.ValueObjects.TenantId.Create(tenantId),
            "dupeuser",
            SharedKernel.Domain.ValueObjects.Email.Create("other@example.com"),
            "Other", "User");

        _userRepository.GetByEmailAsync("unique@example.com", Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _userRepository.GetByUsernameAsync("dupeuser", tenantId, Arg.Any<CancellationToken>())
            .Returns(existingUser);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.Conflict");
        result.Error.Description.Should().Contain("already taken");
        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithPhoneNumber_ShouldCreateUserWithPhone()
    {
        var tenantId = Guid.NewGuid().ToString("N");
        var command = new CreateUserCommand(tenantId, "phoneuser", "phone@example.com", "Phone", "User", "712345678", "+967", null);

        _userRepository.GetByEmailAsync("phone@example.com", Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _userRepository.GetByUsernameAsync("phoneuser", tenantId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.PhoneNumber.Should().Be("+967712345678");
    }

    [Fact]
    public async Task Handle_WithExternalId_ShouldCreateUserWithExternalId()
    {
        var tenantId = Guid.NewGuid().ToString("N");
        var command = new CreateUserCommand(tenantId, "extuser", "ext@example.com", "Ext", "User", null, null, "ext-456");

        _userRepository.GetByEmailAsync("ext@example.com", Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _userRepository.GetByUsernameAsync("extuser", tenantId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ExternalId.Should().Be("ext-456");
    }

    [Fact]
    public async Task Handle_WithInvalidEmail_ShouldThrow()
    {
        var tenantId = Guid.NewGuid().ToString("N");
        var command = new CreateUserCommand(tenantId, "testuser", "not-an-email", "Test", "User", null, null, null);

        _userRepository.GetByEmailAsync("not-an-email", Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }
}
