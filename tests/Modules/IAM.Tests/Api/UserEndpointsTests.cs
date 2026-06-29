using Xunit;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using Obss.IAM.Application.Commands.AssignRole;
using Obss.IAM.Application.Commands.CreateUser;
using Obss.IAM.Application.Commands.DeactivateUser;
using Obss.IAM.Application.Commands.UpdateUser;
using Obss.IAM.Application.DTOs;
using Obss.IAM.Application.Queries.GetUserById;
using Obss.IAM.Application.Queries.GetUsers;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.IAM.Tests.Api;

public class UserEndpointsTests
{
    private readonly IMediator _mediator;

    public UserEndpointsTests()
    {
        _mediator = Substitute.For<IMediator>();
    }

    private static async Task<IResult> PostUser(CreateUserCommand command, IMediator mediator)
    {
        var result = await mediator.Send(command);
        return result.IsSuccess
            ? Results.Created($"/api/v1/iam/users/{result.Value.Id}", result.Value)
            : Results.BadRequest(result.Error);
    }

    private static async Task<IResult> GetUserById(Guid id, IMediator mediator)
    {
        var result = await mediator.Send(new GetUserByIdQuery(id));
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.NotFound(result.Error);
    }

    private static async Task<IResult> GetUsers(GetUsersQuery query, IMediator mediator)
    {
        var result = await mediator.Send(query);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(result.Error);
    }

    private static async Task<IResult> UpdateUser(Guid id, UpdateUserCommand command, IMediator mediator)
    {
        if (id != command.UserId)
            return Results.BadRequest();
        var result = await mediator.Send(command);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(result.Error);
    }

    private static async Task<IResult> DeactivateUser(Guid id, IMediator mediator)
    {
        var result = await mediator.Send(new DeactivateUserCommand(id));
        return result.IsSuccess
            ? Results.NoContent()
            : Results.BadRequest(result.Error);
    }

    private static async Task<IResult> AssignRole(Guid id, AssignRoleCommand command, IMediator mediator)
    {
        if (id != command.UserId)
            return Results.BadRequest();
        var result = await mediator.Send(command);
        return result.IsSuccess
            ? Results.NoContent()
            : Results.BadRequest(result.Error);
    }

    [Fact]
    public async Task PostUser_WithValidCommand_ReturnsCreated()
    {
        var command = new CreateUserCommand("tenant1", "johndoe", "john@example.com", "John", "Doe", null, null, null);
        var userDto = new UserDto(Guid.NewGuid(), "tenant1", "johndoe", "john@example.com", "John", "Doe",
            null, true, false, null, null, DateTime.UtcNow, DateTime.UtcNow, []);
        _mediator.Send(command, Arg.Any<CancellationToken>())
            .Returns(Result.Success(userDto));

        var response = await PostUser(command, _mediator);

        response.Should().BeOfType<Created<UserDto>>();
        var created = (Created<UserDto>)response;
        created.Value.Should().Be(userDto);
        created.Location.Should().Contain(userDto.Id.ToString());
    }

    [Fact]
    public async Task PostUser_WithInvalidCommand_ReturnsBadRequest()
    {
        var command = new CreateUserCommand("tenant1", "johndoe", "john@example.com", "John", "Doe", null, null, null);
        var error = Error.Conflict("Email already registered");
        _mediator.Send(command, Arg.Any<CancellationToken>())
            .Returns(Result.Failure<UserDto>(error));

        var response = await PostUser(command, _mediator);

        response.Should().BeOfType<BadRequest<Error>>();
        var badRequest = (BadRequest<Error>)response;
        badRequest.Value.Should().Be(error);
    }

    [Fact]
    public async Task GetUserById_WithExistingId_ReturnsOk()
    {
        var userId = Guid.NewGuid();
        var userDto = new UserDto(userId, "tenant1", "johndoe", "john@example.com", "John", "Doe",
            null, true, false, null, null, DateTime.UtcNow, DateTime.UtcNow, []);
        _mediator.Send(Arg.Is<GetUserByIdQuery>(q => q.UserId == userId), Arg.Any<CancellationToken>())
            .Returns(Result.Success(userDto));

        var response = await GetUserById(userId, _mediator);

        response.Should().BeOfType<Ok<UserDto>>();
        var ok = (Ok<UserDto>)response;
        ok.Value.Should().Be(userDto);
    }

    [Fact]
    public async Task GetUserById_WithNonExistingId_ReturnsNotFound()
    {
        var userId = Guid.NewGuid();
        var error = Error.NotFound("User", userId);
        _mediator.Send(Arg.Is<GetUserByIdQuery>(q => q.UserId == userId), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<UserDto>(error));

        var response = await GetUserById(userId, _mediator);

        response.Should().BeOfType<NotFound<Error>>();
        var notFound = (NotFound<Error>)response;
        notFound.Value.Should().Be(error);
    }

    [Fact]
    public async Task GetUsers_WithValidQuery_ReturnsOk()
    {
        var query = new GetUsersQuery(null, null, null, 1, 20);
        var users = new List<UserDto>
        {
            new(Guid.NewGuid(), "tenant1", "user1", "user1@example.com", "User", "One",
                null, true, false, null, null, DateTime.UtcNow, DateTime.UtcNow, [])
        };
        _mediator.Send(query, Arg.Any<CancellationToken>())
            .Returns(Result.Success<IReadOnlyList<UserDto>>(users));

        var response = await GetUsers(query, _mediator);

        response.Should().BeOfType<Ok<IReadOnlyList<UserDto>>>();
        var ok = (Ok<IReadOnlyList<UserDto>>)response;
        ok.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task UpdateUser_WithMatchingId_ReturnsOk()
    {
        var userId = Guid.NewGuid();
        var command = new UpdateUserCommand(userId, "NewName", "NewLast", null, null);
        var userDto = new UserDto(userId, "tenant1", "johndoe", "john@example.com", "NewName", "NewLast",
            null, true, false, null, null, DateTime.UtcNow, DateTime.UtcNow, []);
        _mediator.Send(command, Arg.Any<CancellationToken>())
            .Returns(Result.Success(userDto));

        var response = await UpdateUser(userId, command, _mediator);

        response.Should().BeOfType<Ok<UserDto>>();
        var ok = (Ok<UserDto>)response;
        ok.Value.Should().Be(userDto);
    }

    [Fact]
    public async Task UpdateUser_WithNonMatchingId_ReturnsBadRequest()
    {
        var userId = Guid.NewGuid();
        var command = new UpdateUserCommand(Guid.NewGuid(), "NewName", "NewLast", null, null);

        var response = await UpdateUser(userId, command, _mediator);

        response.Should().BeOfType<BadRequest>();
    }

    [Fact]
    public async Task DeactivateUser_WithExistingUser_ReturnsNoContent()
    {
        var userId = Guid.NewGuid();
        _mediator.Send(Arg.Is<DeactivateUserCommand>(c => c.UserId == userId), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var response = await DeactivateUser(userId, _mediator);

        response.Should().BeOfType<NoContent>();
    }

    [Fact]
    public async Task DeactivateUser_WithNonExistingUser_ReturnsBadRequest()
    {
        var userId = Guid.NewGuid();
        var error = Error.NotFound("User", userId);
        _mediator.Send(Arg.Is<DeactivateUserCommand>(c => c.UserId == userId), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(error));

        var response = await DeactivateUser(userId, _mediator);

        response.Should().BeOfType<BadRequest<Error>>();
        var badRequest = (BadRequest<Error>)response;
        badRequest.Value.Should().Be(error);
    }

    [Fact]
    public async Task AssignRole_WithMatchingId_ReturnsNoContent()
    {
        var userId = Guid.NewGuid();
        var command = new AssignRoleCommand(userId, Guid.NewGuid(), Guid.NewGuid());
        _mediator.Send(command, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var response = await AssignRole(userId, command, _mediator);

        response.Should().BeOfType<NoContent>();
    }

    [Fact]
    public async Task AssignRole_WithNonMatchingId_ReturnsBadRequest()
    {
        var userId = Guid.NewGuid();
        var command = new AssignRoleCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        var response = await AssignRole(userId, command, _mediator);

        response.Should().BeOfType<BadRequest>();
    }
}
