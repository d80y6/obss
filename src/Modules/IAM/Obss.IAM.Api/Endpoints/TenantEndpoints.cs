using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.IAM.Application.Commands.CreateTenant;
using Obss.IAM.Application.Queries.GetTenants;

namespace Obss.IAM.Api.Endpoints;

public static class TenantEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/tenants", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetTenantsQuery());
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/tenants", async (CreateTenantCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/iam/tenants/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });
    }
}