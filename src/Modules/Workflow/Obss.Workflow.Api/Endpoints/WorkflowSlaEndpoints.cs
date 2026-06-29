using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.Workflow.Application.Commands.ApplyWorkflowSla;
using Obss.Workflow.Application.Commands.CreateWorkflowSla;
using Obss.Workflow.Application.Queries.GetSlaBreachedWorkflows;
using Obss.Workflow.Application.Queries.GetWorkflowSlas;
using Obss.Workflow.Application.Queries.GetWorkflowSlaStatus;

namespace Obss.Workflow.Api.Endpoints;

public static class WorkflowSlaEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/slas", async (CreateWorkflowSlaCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/workflow/slas/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/slas", async ([AsParameters] GetWorkflowSlasQuery query, IMediator mediator) =>
        {
            var result = await mediator.Send(query);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/instances/{id:guid}/apply-sla", async (Guid id, ApplyWorkflowSlaCommand command, IMediator mediator) =>
        {
            if (id != command.WorkflowInstanceId)
                return (IResult)TypedResults.BadRequest();

            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/sla/breached", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetSlaBreachedWorkflowsQuery());
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/sla/status/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetWorkflowSlaStatusQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        });
    }
}
