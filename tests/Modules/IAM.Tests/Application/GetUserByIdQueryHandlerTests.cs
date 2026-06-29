using Xunit;
using FluentAssertions;
using NSubstitute;
using Obss.IAM.Application.Abstractions;
using Obss.IAM.Application.Queries.GetUserById;
using Obss.IAM.Domain.Entities;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.IAM.Tests.Application;

public class GetUserByIdQueryHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly GetUserByIdQueryHandler _handler;

    public GetUserByIdQueryHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _handler = new GetUserByIdQueryHandler(_userRepository);
    }

    [Fact]
    public async Task Handle_WithExistingUser_ShouldReturnUser()
    {
        var userId = Guid.NewGuid();
        var query = new GetUserByIdQuery(userId);
        var user = User.Create(
            SharedKernel.Domain.ValueObjects.TenantId.Create(Guid.NewGuid().ToString("N")),
            "testuser",
            SharedKernel.Domain.ValueObjects.Email.Create("test@example.com"),
            "Test", "User");

        _userRepository.GetByIdWithRolesAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(user.Id);
        result.Value.Username.Should().Be("testuser");
        result.Value.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task Handle_WithNonExistingUser_ShouldReturnNotFound()
    {
        var userId = Guid.NewGuid();
        var query = new GetUserByIdQuery(userId);

        _userRepository.GetByIdWithRolesAsync(userId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.NotFound");
        result.Error.Description.Should().Contain(nameof(User));
    }

    [Fact]
    public async Task Handle_ShouldCallGetByIdWithRolesAsync()
    {
        var userId = Guid.NewGuid();
        var query = new GetUserByIdQuery(userId);
        var user = User.Create(
            SharedKernel.Domain.ValueObjects.TenantId.Create(Guid.NewGuid().ToString("N")),
            "testuser",
            SharedKernel.Domain.ValueObjects.Email.Create("test@example.com"),
            "Test", "User");

        _userRepository.GetByIdWithRolesAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        await _handler.Handle(query, CancellationToken.None);

        await _userRepository.Received(1).GetByIdWithRolesAsync(userId, Arg.Any<CancellationToken>());
    }
}
