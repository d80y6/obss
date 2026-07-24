using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.NetworkInventory.Application.Commands.SaveTopologyMap;
using Obss.NetworkInventory.Application.Queries.GetNetworkTopology;
using Obss.NetworkInventory.Application.Queries.GetTopologyMaps;
using Obss.SharedKernel.Application.Authorization;

namespace Obss.NetworkInventory.Api.Endpoints;

public static class TopologyEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/topology", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetNetworkTopologyQuery());
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Network.TopologyRead));

        group.MapPost("/topology/maps", async (SaveTopologyMapCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/network/topology/maps/{result.Value}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Network.TopologyRead));

        group.MapGet("/topology/maps", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetTopologyMapsQuery());
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Network.TopologyRead));
    }
}
