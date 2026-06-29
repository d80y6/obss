using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.Collections.Application.Commands.AddCollectionAction;
using Obss.Collections.Application.Commands.CreatePaymentArrangement;
using Obss.Collections.Application.Commands.OpenCollectionCase;
using Obss.Collections.Application.Commands.RecordArrangementPayment;
using Obss.Collections.Application.Commands.ResolveCollectionCase;
using Obss.Collections.Application.Commands.SendDunningNotice;
using Obss.Collections.Application.Queries.GetAgingReport;
using Obss.Collections.Application.Queries.GetCollectionCaseById;
using Obss.Collections.Application.Queries.GetCollectionCases;

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
        });

        group.MapGet("/cases/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetCollectionCaseByIdQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        });

        group.MapGet("/cases", async ([AsParameters] GetCollectionCasesQuery query, IMediator mediator) =>
        {
            var result = await mediator.Send(query);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/cases/{id:guid}/actions", async (Guid id, AddCollectionActionCommand command, IMediator mediator) =>
        {
            if (id != command.CollectionCaseId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/cases/{id:guid}/arrangements", async (Guid id, CreatePaymentArrangementCommand command, IMediator mediator) =>
        {
            if (id != command.CollectionCaseId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/collections/cases/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/arrangements/{arrangementId:guid}/payments", async (Guid arrangementId, RecordArrangementPaymentCommand command, IMediator mediator) =>
        {
            if (arrangementId != command.PaymentArrangementId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/cases/{id:guid}/resolve", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new ResolveCollectionCaseCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/cases/{id:guid}/dunning", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new SendDunningNoticeCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/reports/aging", async ([AsParameters] GetAgingReportQuery query, IMediator mediator) =>
        {
            var result = await mediator.Send(query);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });
    }
}
