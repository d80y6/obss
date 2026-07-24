using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.SharedKernel.Application.Authorization;
using Obss.Workflow.Application.Commands.CompleteWorkflowInstance;
using Obss.Workflow.Application.Commands.ExecuteWorkflowTask;
using Obss.Workflow.Application.Commands.FailWorkflowInstance;
using Obss.Workflow.Application.Commands.StartWorkflowInstance;
using Obss.Workflow.Application.Queries.GetPendingTasks;
using Obss.Workflow.Application.Queries.GetWorkflowInstanceById;
using Obss.Workflow.Application.Queries.GetWorkflowInstances;

namespace Obss.Workflow.Api.Endpoints;

public static class WorkflowInstanceEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/instances", async (StartWorkflowInstanceCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/workflow/instances/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Telecom.ServiceWrite));

        group.MapGet("/instances/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetWorkflowInstanceByIdQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Telecom.ServiceRead));

        group.MapGet("/instances", async ([AsParameters] GetWorkflowInstancesQuery query, IMediator mediator) =>
        {
            var result = await mediator.Send(query);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Telecom.ServiceRead));

        group.MapGet("/tasks/pending", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetPendingTasksQuery());
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Telecom.ServiceRead));

        group.MapPost("/instances/{id:guid}/execute/{taskId:guid}", async (Guid id, Guid taskId, IMediator mediator) =>
        {
            var result = await mediator.Send(new ExecuteWorkflowTaskCommand(id, taskId));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Telecom.ServiceWrite));

        group.MapPost("/instances/{id:guid}/complete", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new CompleteWorkflowInstanceCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Telecom.ServiceWrite));

        group.MapPost("/instances/{id:guid}/fail", async (Guid id, FailWorkflowInstanceCommand command, IMediator mediator) =>
        {
            if (id != command.InstanceId)
                return (IResult)TypedResults.BadRequest();

            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Telecom.ServiceWrite));
    }
}
