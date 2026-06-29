using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.NetworkInventory.Application.Commands.CreateConnectivityLink;
using Obss.NetworkInventory.Application.Commands.UpdateLinkStatus;
using Obss.NetworkInventory.Application.Queries.GetDegradedLinks;
using Obss.NetworkInventory.Application.Queries.GetElementConnections;

namespace Obss.NetworkInventory.Api.Endpoints;

public static class LinkEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/links", async (CreateConnectivityLinkCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/network/links/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/elements/{id:guid}/connections", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetElementConnectionsQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        });

        group.MapGet("/links/degraded", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetDegradedLinksQuery());
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPatch("/links/{id:guid}/status", async (Guid id, UpdateLinkStatusCommand command, IMediator mediator) =>
        {
            if (id != command.Id)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });
    }
}
