using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.NetworkInventory.Application.Commands.AddInterface;
using Obss.NetworkInventory.Application.Commands.AllocateIP;
using Obss.NetworkInventory.Application.Commands.CreateNetworkElement;
using Obss.NetworkInventory.Application.Commands.UpdateNetworkElement;
using Obss.NetworkInventory.Application.Queries.GetNetworkElementById;
using Obss.NetworkInventory.Application.Queries.GetNetworkElements;
using Obss.SharedKernel.Application.Authorization;

namespace Obss.NetworkInventory.Api.Endpoints;

public static class NetworkElementEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/elements", async (CreateNetworkElementCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/network/elements/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Network.ElementWrite));

        group.MapGet("/elements/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetNetworkElementByIdQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Network.ElementRead));

        group.MapGet("/elements", async ([AsParameters] GetNetworkElementsQuery query, IMediator mediator) =>
        {
            var result = await mediator.Send(query);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Network.ElementRead));

        group.MapPut("/elements/{id:guid}", async (Guid id, UpdateNetworkElementCommand command, IMediator mediator) =>
        {
            if (id != command.Id)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Network.ElementWrite));

        group.MapPost("/elements/{id:guid}/interfaces", async (Guid id, AddInterfaceCommand command, IMediator mediator) =>
        {
            if (id != command.NetworkElementId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/network/elements/{id}/interfaces/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Network.ElementWrite));

        group.MapPost("/elements/{id:guid}/ip-addresses", async (Guid id, AllocateIPCommand command, IMediator mediator) =>
        {
            if (id != command.NetworkElementId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/network/elements/{id}/ip-addresses/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Network.ElementWrite));

        group.MapPost("/elements/{id:guid}/activate", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new UpdateNetworkElementCommand(id, default!, default!, default!, default!, null, null, null, null, null, false));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Network.ElementWrite));
    }
}
