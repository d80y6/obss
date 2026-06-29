using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.IAM.Application.Commands.CreateUser;
using Obss.IAM.Application.Commands.DeactivateUser;
using Obss.IAM.Application.Commands.UpdateUser;
using Obss.IAM.Application.Commands.AssignRole;
using Obss.IAM.Application.Queries.GetUserById;
using Obss.IAM.Application.Queries.GetUsers;

namespace Obss.IAM.Api.Endpoints;

public static class UserEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/users", async (CreateUserCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/iam/users/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/users/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetUserByIdQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        });

        group.MapGet("/users", async ([AsParameters] GetUsersQuery query, IMediator mediator) =>
        {
            var result = await mediator.Send(query);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPut("/users/{id:guid}", async (Guid id, UpdateUserCommand command, IMediator mediator) =>
        {
            if (id != command.UserId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/users/{id:guid}/deactivate", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new DeactivateUserCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/users/{id:guid}/roles", async (Guid id, AssignRoleCommand command, IMediator mediator) =>
        {
            if (id != command.UserId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });
    }
}