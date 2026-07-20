using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.IAM.Application.Commands.AddRolePermission;
using Obss.IAM.Application.Commands.CreateRole;
using Obss.IAM.Application.Commands.DeleteRole;
using Obss.IAM.Application.Commands.RemoveRolePermission;
using Obss.IAM.Application.Commands.UpdateRole;
using Obss.IAM.Application.Queries.GetRoleById;
using Obss.IAM.Application.Queries.GetRoles;
using Obss.SharedKernel.Application.Authorization;

namespace Obss.IAM.Api.Endpoints;

public static class RoleEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/roles", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetRolesQuery());
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Iam.RoleRead));

        group.MapGet("/roles/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetRoleByIdQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Iam.RoleRead));

        group.MapPost("/roles", async (CreateRoleCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/iam/roles/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Iam.RoleWrite));

        group.MapPut("/roles/{id:guid}", async (Guid id, UpdateRoleCommand command, IMediator mediator) =>
        {
            if (id != command.RoleId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Iam.RoleWrite));

        group.MapDelete("/roles/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new DeleteRoleCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Iam.RoleWrite));

        group.MapPost("/roles/{id:guid}/permissions", async (Guid id, AddRolePermissionCommand command, IMediator mediator) =>
        {
            if (id != command.RoleId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Iam.PermissionManage));

        group.MapDelete("/roles/{id:guid}/permissions/{permissionId:guid}", async (Guid id, Guid permissionId, IMediator mediator) =>
        {
            var result = await mediator.Send(new RemoveRolePermissionCommand(id, permissionId));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Iam.PermissionManage));
    }
}
