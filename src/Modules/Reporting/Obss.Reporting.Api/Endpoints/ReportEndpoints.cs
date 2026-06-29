using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.Reporting.Application.Commands.CreateReportDefinition;
using Obss.Reporting.Application.Commands.ExecuteReport;
using Obss.Reporting.Application.Commands.ScheduleReport;
using Obss.Reporting.Application.Queries.GetReportDefinitions;
using Obss.Reporting.Application.Queries.GetReportDefinitionById;
using Obss.Reporting.Application.Queries.GetReportExecutions;
using Obss.Reporting.Application.Queries.GetScheduledReportById;
using Obss.Reporting.Application.Queries.GetScheduledReports;

namespace Obss.Reporting.Api.Endpoints;

public static class ReportEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/definitions", async (CreateReportDefinitionCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/reporting/definitions/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/definitions", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetReportDefinitionsQuery());
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/definitions/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetReportDefinitionByIdQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        });

        group.MapGet("/schedule/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetScheduledReportByIdQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        });

        group.MapPost("/definitions/{id:guid}/execute", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new ExecuteReportCommand(id, "api"));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/schedule", async (ScheduleReportCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/reporting/schedule/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/definitions/{id:guid}/executions", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetReportExecutionsQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        });

        group.MapGet("/schedule", async ([AsParameters] GetScheduledReportsQuery query, IMediator mediator) =>
        {
            var result = await mediator.Send(query);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });
    }
}
