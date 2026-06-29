using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.Workflow.Application.Queries.GetRunningWorkflows;
using Obss.Workflow.Application.Queries.GetWorkflowDashboard;
using Obss.Workflow.Application.Queries.GetWorkflowMetrics;

namespace Obss.Workflow.Api.Endpoints;

public static class WorkflowMonitoringEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/dashboard", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetWorkflowDashboardQuery());
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/metrics", async ([AsParameters] GetWorkflowMetricsQuery query, IMediator mediator) =>
        {
            var result = await mediator.Send(query);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/instances/running", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetRunningWorkflowsQuery());
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });
    }
}
