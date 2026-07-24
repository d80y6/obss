using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Obss.NetworkInventory.Application.Commands.CreateOLT;
using Obss.NetworkInventory.Application.Commands.RegisterONT;
using Obss.NetworkInventory.Application.Queries.GetNetworkElementById;
using Obss.NetworkInventory.Application.Queries.GetNetworkElements;
using Obss.NetworkInventory.Infrastructure.Persistence;
using Obss.SharedKernel.Application.Authorization;

namespace Obss.NetworkInventory.Api.Endpoints;

public static class OLTEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/olts", async (CreateOLTCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/network/olts/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Network.ElementWrite));

        group.MapPost("/olts/{id:guid}/register-ont", async (Guid id, RegisterONTCommand command, IMediator mediator) =>
        {
            if (id != command.OLTId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Network.ElementWrite));

        group.MapGet("/olts/{id:guid}/ports", async (Guid id, NetworkDbContext dbContext) =>
        {
            var ports = await dbContext.PONPorts
                .Where(p => p.OLTId == id)
                .OrderBy(p => p.PortNumber)
                .ToListAsync();
            return (IResult)TypedResults.Ok(ports);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Network.ElementRead));

        group.MapGet("/olts", async ([AsParameters] GetNetworkElementsQuery query, IMediator mediator) =>
        {
            var typedQuery = query with { Type = "OLT" };
            var result = await mediator.Send(typedQuery);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Network.ElementRead));

        group.MapGet("/olts/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetNetworkElementByIdQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Network.ElementRead));
    }
}
