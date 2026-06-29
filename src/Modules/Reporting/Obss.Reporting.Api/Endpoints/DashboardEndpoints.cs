using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.Reporting.Application.Commands.CreateDashboardWidget;
using Obss.Reporting.Application.Queries.GetDashboardConfig;

namespace Obss.Reporting.Api.Endpoints;

public static class DashboardEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/widgets", async (CreateDashboardWidgetCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/reporting/widgets/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/dashboard", async ([AsParameters] GetDashboardConfigQuery query, IMediator mediator) =>
        {
            var result = await mediator.Send(query);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });
    }
}
