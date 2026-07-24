using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.SharedKernel.Application.Authorization;
using Obss.Workflow.Application.Commands.AddWorkflowStep;
using Obss.Workflow.Application.Commands.CreateWorkflowDefinition;
using Obss.Workflow.Application.Commands.RemoveWorkflowStep;
using Obss.Workflow.Application.Queries.GetWorkflowDefinitionById;
using Obss.Workflow.Application.Queries.GetWorkflowDefinitions;

namespace Obss.Workflow.Api.Endpoints;

public static class WorkflowDefinitionEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/definitions", async (CreateWorkflowDefinitionCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/workflow/definitions/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Telecom.ServiceWrite));

        group.MapGet("/definitions/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetWorkflowDefinitionByIdQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Telecom.ServiceRead));

        group.MapGet("/definitions", async ([AsParameters] GetWorkflowDefinitionsQuery query, IMediator mediator) =>
        {
            var result = await mediator.Send(query);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Telecom.ServiceRead));

        group.MapPost("/definitions/{id:guid}/steps", async (Guid id, AddWorkflowStepCommand command, IMediator mediator) =>
        {
            if (id != command.WorkflowDefinitionId)
                return (IResult)TypedResults.BadRequest();

            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Telecom.ServiceWrite));

        group.MapDelete("/definitions/{id:guid}/steps/{stepId:guid}", async (Guid id, Guid stepId, IMediator mediator) =>
        {
            var result = await mediator.Send(new RemoveWorkflowStepCommand(id, stepId));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Telecom.ServiceWrite));
    }
}
