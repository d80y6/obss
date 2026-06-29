using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.NetworkInventory.Application.Commands.CreateSubnet;
using Obss.NetworkInventory.Application.Queries.GetSubnets;

namespace Obss.NetworkInventory.Api.Endpoints;

public static class SubnetEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/subnets", async (CreateSubnetCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/network/subnets/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/subnets", async ([AsParameters] GetSubnetsQuery query, IMediator mediator) =>
        {
            var result = await mediator.Send(query);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });
    }
}
