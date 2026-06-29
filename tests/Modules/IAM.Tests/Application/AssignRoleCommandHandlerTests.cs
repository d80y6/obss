using Xunit;
using FluentAssertions;
using NSubstitute;
using Obss.IAM.Application.Commands.AssignRole;
using Obss.IAM.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.IAM.Tests.Application;

public class AssignRoleCommandHandlerTests
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Role> _roleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AssignRoleCommandHandler _handler;

    public AssignRoleCommandHandlerTests()
    {
        _userRepository = Substitute.For<IRepository<User>>();
        _roleRepository = Substitute.For<IRepository<Role>>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new AssignRoleCommandHandler(_userRepository, _roleRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WithValidUserAndRole_ShouldAssign()
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var assignedBy = Guid.NewGuid();
        var command = new AssignRoleCommand(userId, roleId, assignedBy);

        var user = User.Create(
            SharedKernel.Domain.ValueObjects.TenantId.Create(Guid.NewGuid().ToString("N")),
            "testuser",
            SharedKernel.Domain.ValueObjects.Email.Create("test@example.com"),
            "Test", "User");
        var role = new Role(roleId, Guid.NewGuid().ToString("N"), "Admin", "Admin role");

        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);
        _roleRepository.GetByIdAsync(roleId, Arg.Any<CancellationToken>())
            .Returns(role);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        user.UserRoles.Should().ContainSingle(ur => ur.RoleId == roleId);
        await _userRepository.Received(1).UpdateAsync(user, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistingUser_ShouldReturnNotFound()
    {
        var command = new AssignRoleCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        _userRepository.GetByIdAsync(command.UserId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.NotFound");
        result.Error.Description.Should().Contain(nameof(User));
        await _userRepository.DidNotReceive().UpdateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistingRole_ShouldReturnNotFound()
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var command = new AssignRoleCommand(userId, roleId, Guid.NewGuid());

        var user = User.Create(
            SharedKernel.Domain.ValueObjects.TenantId.Create(Guid.NewGuid().ToString("N")),
            "testuser",
            SharedKernel.Domain.ValueObjects.Email.Create("test@example.com"),
            "Test", "User");

        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);
        _roleRepository.GetByIdAsync(roleId, Arg.Any<CancellationToken>())
            .Returns((Role?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.NotFound");
        result.Error.Description.Should().Contain(nameof(Role));
        await _userRepository.DidNotReceive().UpdateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRoleAlreadyAssigned_ShouldNotDuplicate()
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var assignedBy = Guid.NewGuid();
        var command = new AssignRoleCommand(userId, roleId, assignedBy);

        var user = User.Create(
            SharedKernel.Domain.ValueObjects.TenantId.Create(Guid.NewGuid().ToString("N")),
            "testuser",
            SharedKernel.Domain.ValueObjects.Email.Create("test@example.com"),
            "Test", "User");
        user.AssignRole(roleId, assignedBy);
        var role = new Role(roleId, Guid.NewGuid().ToString("N"), "Admin", "Admin role");

        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);
        _roleRepository.GetByIdAsync(roleId, Arg.Any<CancellationToken>())
            .Returns(role);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        user.UserRoles.Should().ContainSingle(ur => ur.RoleId == roleId);
    }
}
