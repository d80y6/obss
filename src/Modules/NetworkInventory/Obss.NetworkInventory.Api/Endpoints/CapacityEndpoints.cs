using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.NetworkInventory.Application.Commands.RecordCapacity;
using Obss.NetworkInventory.Application.Queries.GetCapacityAlerts;
using Obss.NetworkInventory.Application.Queries.GetElementCapacity;
using Obss.NetworkInventory.Application.Queries.GetOverallNetworkCapacity;

namespace Obss.NetworkInventory.Api.Endpoints;

public static class CapacityEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/elements/{id:guid}/capacity", async (Guid id, RecordCapacityCommand command, IMediator mediator) =>
        {
            if (id != command.ElementId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/network/elements/{id}/capacity/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/elements/{id:guid}/capacity", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetElementCapacityQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        });

        group.MapGet("/capacity/alerts", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetCapacityAlertsQuery());
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/capacity/overview", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetOverallNetworkCapacityQuery());
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });
    }
}
