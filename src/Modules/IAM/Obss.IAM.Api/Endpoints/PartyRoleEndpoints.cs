using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.IAM.Application.Commands;
using Obss.IAM.Application.Queries;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Infrastructure;

namespace Obss.IAM.Api.Endpoints;

public static class PartyRoleEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/party-roles", async (CreatePartyRoleCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/iam/party-roles/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/party-roles/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetPartyRoleByIdQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        });

        group.MapGet("/party-roles", async ([AsParameters] GetPartyRolesQuery query, IMediator mediator, HttpContext httpContext) =>
        {
            var result = await mediator.Send(query);
            if (!result.IsSuccess)
                return (IResult)TypedResults.BadRequest(result.Error);

            var paginationRequest = new TmfPaginationRequest { Offset = query.Offset, Limit = query.Limit };
            httpContext.Response.SetPaginationHeaders(paginationRequest, result.Value.TotalCount);
            return (IResult)TypedResults.Ok(result.Value.Items);
        });

        group.MapPut("/party-roles/{id:guid}", async (Guid id, UpdatePartyRoleCommand command, IMediator mediator) =>
        {
            if (id != command.Id)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/party-roles/{id:guid}/suspend", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new SuspendPartyRoleCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapDelete("/party-roles/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new DeletePartyRoleCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });
    }
}
