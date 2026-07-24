using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.NetworkInventory.Application.Commands.CreateVLAN;
using Obss.NetworkInventory.Application.Queries.GetNetworkElementById;
using Obss.NetworkInventory.Application.Queries.GetNetworkElements;
using Obss.SharedKernel.Application.Authorization;

namespace Obss.NetworkInventory.Api.Endpoints;

public static class VLANEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/vlans", async (CreateVLANCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/network/vlans/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Network.ElementWrite));

        group.MapGet("/vlans", async ([AsParameters] GetNetworkElementsQuery query, IMediator mediator) =>
        {
            var typedQuery = query with { Type = "VLAN" };
            var result = await mediator.Send(typedQuery);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Network.ElementRead));

        group.MapGet("/vlans/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetNetworkElementByIdQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Network.ElementRead));
    }
}
