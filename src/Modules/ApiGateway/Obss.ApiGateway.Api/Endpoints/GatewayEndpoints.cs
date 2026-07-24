using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.ApiGateway.Application.Commands.CreateApiKey;
using Obss.ApiGateway.Application.Commands.RegisterApiRoute;
using Obss.ApiGateway.Application.Commands.RegisterPartner;
using Obss.ApiGateway.Application.Commands.RevokeApiKey;
using Obss.ApiGateway.Application.Queries.GetApiKeys;
using Obss.ApiGateway.Application.Queries.GetApiRoutes;
using Obss.ApiGateway.Application.Queries.GetPartners;
using Obss.SharedKernel.Application.Authorization;

namespace Obss.ApiGateway.Api.Endpoints;

public static class GatewayEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/routes", async (RegisterApiRouteCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/gateway/routes/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Telecom.AdapterManage));

        group.MapGet("/routes", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetApiRoutesQuery());
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Telecom.AdapterRead));

        group.MapPost("/api-keys", async (CreateApiKeyCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/gateway/api-keys/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Telecom.AdapterManage));

        group.MapGet("/api-keys", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetApiKeysQuery());
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Telecom.AdapterRead));

        group.MapPost("/api-keys/{id:guid}/revoke", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new RevokeApiKeyCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Telecom.AdapterManage));

        group.MapPost("/partners", async (RegisterPartnerCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/gateway/partners/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Telecom.AdapterManage));

        group.MapGet("/partners", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetPartnersQuery());
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Telecom.AdapterRead));
    }
}
