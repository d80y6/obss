using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.Collections.Application.Commands.AddCollectionAction;
using Obss.Collections.Application.Commands.CreateDunningPolicy;
using Obss.Collections.Application.Commands.CreatePaymentArrangement;
using Obss.Collections.Application.Commands.DeleteDunningPolicy;
using Obss.Collections.Application.Commands.OpenCollectionCase;
using Obss.Collections.Application.Commands.RecordArrangementPayment;
using Obss.Collections.Application.Commands.ResolveCollectionCase;
using Obss.Collections.Application.Commands.SendDunningNotice;
using Obss.Collections.Application.Commands.UpdateDunningPolicy;
using Obss.Collections.Application.Queries.GetAgingReport;
using Obss.Collections.Application.Queries.GetCollectionCaseById;
using Obss.Collections.Application.Queries.GetCollectionCases;
using Obss.Collections.Application.Queries.GetDunningPolicies;
using Obss.Collections.Application.Queries.GetDunningPolicyById;
using Obss.SharedKernel.Application.Authorization;

namespace Obss.Collections.Api.Endpoints;

public static class CollectionEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/cases", async (OpenCollectionCaseCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/collections/cases/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Collections.CaseWrite));

        group.MapGet("/cases/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetCollectionCaseByIdQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Collections.CaseRead));

        group.MapGet("/cases", async ([AsParameters] GetCollectionCasesQuery query, IMediator mediator) =>
        {
            var result = await mediator.Send(query);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Collections.CaseRead));

        group.MapPost("/cases/{id:guid}/actions", async (Guid id, AddCollectionActionCommand command, IMediator mediator) =>
        {
            if (id != command.CollectionCaseId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Collections.CaseWrite));

        group.MapPost("/cases/{id:guid}/arrangements", async (Guid id, CreatePaymentArrangementCommand command, IMediator mediator) =>
        {
            if (id != command.CollectionCaseId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/collections/cases/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Collections.ArrangementManage));

        group.MapPost("/arrangements/{arrangementId:guid}/payments", async (Guid arrangementId, RecordArrangementPaymentCommand command, IMediator mediator) =>
        {
            if (arrangementId != command.PaymentArrangementId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Collections.ArrangementManage));

        group.MapPost("/cases/{id:guid}/resolve", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new ResolveCollectionCaseCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Collections.CaseWrite));

        group.MapPost("/cases/{id:guid}/dunning", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new SendDunningNoticeCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Collections.DunningManage));

        group.MapGet("/reports/aging", async ([AsParameters] GetAgingReportQuery query, IMediator mediator) =>
        {
            var result = await mediator.Send(query);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Collections.CaseRead));

        group.MapGet("/dunning-policies", async ([AsParameters] GetDunningPoliciesQuery query, IMediator mediator) =>
        {
            var result = await mediator.Send(query);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Collections.DunningManage));

        group.MapGet("/dunning-policies/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetDunningPolicyByIdQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Collections.DunningManage));

        group.MapPost("/dunning-policies", async (CreateDunningPolicyCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/collections/dunning-policies/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Collections.DunningManage));

        group.MapPut("/dunning-policies/{id:guid}", async (Guid id, UpdateDunningPolicyCommand command, IMediator mediator) =>
        {
            if (id != command.Id)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Collections.DunningManage));

        group.MapDelete("/dunning-policies/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new DeleteDunningPolicyCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Collections.DunningManage));
    }
}
