using Mapster;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.Audit.Application.Abstractions;
using Obss.Audit.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.Audit.Application.Commands.AcknowledgeAlert;
using Obss.Audit.Application.Commands.CreateAlertRule;
using Obss.Audit.Application.Commands.CreateAuditEntry;
using Obss.Audit.Application.Commands.CreateAuditPolicy;
using Obss.Audit.Application.Commands.GenerateComplianceReport;
using Obss.Audit.Application.Commands.PurgeOldEntries;
using Obss.Audit.Application.DTOs;
using Obss.Audit.Application.Queries.GetAlerts;
using Obss.Audit.Application.Queries.GetAlertRules;
using Obss.Audit.Application.Queries.GetAuditEntries;
using Obss.Audit.Application.Queries.GetAuditEntryById;
using Obss.Audit.Application.Queries.GetAuditSummary;
using Obss.Audit.Application.Queries.GetComplianceSummary;
using Obss.Audit.Application.Queries.GetEntityAuditTrail;
using Obss.Audit.Application.Queries.GetSensitiveOperations;
using Obss.Audit.Application.Queries.GetUnacknowledgedAlerts;

namespace Obss.Audit.Api.Endpoints;

public static class AuditEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/entries", async (CreateAuditEntryCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/audit/entries/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/entries/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetAuditEntryByIdQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        });

        group.MapGet("/entries", async ([AsParameters] GetAuditEntriesQuery query, IMediator mediator) =>
        {
            var result = await mediator.Send(query);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/entities/{entityType}/{entityId}", async (string entityType, string entityId, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetEntityAuditTrailQuery(entityType, entityId));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/summary", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetAuditSummaryQuery());
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/policies", async (CreateAuditPolicyCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/audit/policies/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/purge", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new PurgeOldEntriesCommand());
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(new { DeletedCount = result.Value })
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/compliance/summary", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetComplianceSummaryQuery());
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/compliance/report", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GenerateComplianceReportCommand());
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/sensitive-operations", async ([AsParameters] GetSensitiveOperationsQuery query, IMediator mediator) =>
        {
            var result = await mediator.Send(query);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/alerts", async ([AsParameters] GetAlertsQuery query, IMediator mediator) =>
        {
            var result = await mediator.Send(query);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/alerts/unacknowledged", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetUnacknowledgedAlertsQuery());
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/alerts/{id:guid}/acknowledge", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new AcknowledgeAlertCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        });

        group.MapPost("/alert-rules", async (CreateAlertRuleCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/audit/alert-rules/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/alert-rules", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetAlertRulesQuery());
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/alert-rules/{id:guid}", async (Guid id, IRepository<AuditAlertRule> repository) =>
        {
            var rule = await repository.GetByIdAsync(id);
            return rule is not null
                ? (IResult)TypedResults.Ok(rule.Adapt<AuditAlertRuleDto>())
                : (IResult)TypedResults.NotFound();
        });
    }
}
