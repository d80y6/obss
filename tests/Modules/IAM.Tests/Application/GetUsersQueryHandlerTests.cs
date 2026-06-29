using Xunit;
using FluentAssertions;
using NSubstitute;
using Obss.IAM.Application.Abstractions;
using Obss.IAM.Application.Queries.GetUsers;
using Obss.IAM.Domain.Entities;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.IAM.Tests.Application;

public class GetUsersQueryHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly GetUsersQueryHandler _handler;

    public GetUsersQueryHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _handler = new GetUsersQueryHandler(_userRepository);
    }

    [Fact]
    public async Task Handle_ShouldReturnFilteredUsers()
    {
        var tenantId = Guid.NewGuid().ToString("N");
        var query = new GetUsersQuery(tenantId, true, null, 1, 20);
        var users = new List<User>
        {
            User.Create(SharedKernel.Domain.ValueObjects.TenantId.Create(tenantId), "user1",
                SharedKernel.Domain.ValueObjects.Email.Create("user1@example.com"), "User", "One"),
            User.Create(SharedKernel.Domain.ValueObjects.TenantId.Create(tenantId), "user2",
                SharedKernel.Domain.ValueObjects.Email.Create("user2@example.com"), "User", "Two"),
        };

        _userRepository.GetFilteredAsync(tenantId, true, null, 1, 20, Arg.Any<CancellationToken>())
            .Returns(users);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(u => u.Username == "user1");
        result.Value.Should().Contain(u => u.Username == "user2");
    }

    [Fact]
    public async Task Handle_WithNoFilters_ShouldReturnAllUsers()
    {
        var query = new GetUsersQuery(null, null, null, 1, 20);
        var users = new List<User>
        {
            User.Create(SharedKernel.Domain.ValueObjects.TenantId.New(), "user1",
                SharedKernel.Domain.ValueObjects.Email.Create("user1@example.com"), "User", "One"),
        };

        _userRepository.GetFilteredAsync(null, null, null, 1, 20, Arg.Any<CancellationToken>())
            .Returns(users);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyListWhenNoUsers()
    {
        var query = new GetUsersQuery(null, null, null, 1, 20);

        _userRepository.GetFilteredAsync(null, null, null, 1, 20, Arg.Any<CancellationToken>())
            .Returns(new List<User>());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithSearchTerm_ShouldPassToRepository()
    {
        var query = new GetUsersQuery(null, null, "john", 1, 10);

        _userRepository.GetFilteredAsync(null, null, "john", 1, 10, Arg.Any<CancellationToken>())
            .Returns(new List<User>());

        await _handler.Handle(query, CancellationToken.None);

        await _userRepository.Received(1).GetFilteredAsync(
            null, null, "john", 1, 10, Arg.Any<CancellationToken>());
    }
}
