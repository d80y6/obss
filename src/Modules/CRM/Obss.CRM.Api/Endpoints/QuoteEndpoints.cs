using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.CRM.Application.Commands.CreateQuote;
using Obss.CRM.Application.Commands.UpdateQuote;
using Obss.CRM.Application.Commands.SubmitQuote;
using Obss.CRM.Application.Commands.ApproveQuote;
using Obss.CRM.Application.Commands.AcceptQuote;
using Obss.CRM.Application.Commands.RejectQuote;
using Obss.CRM.Application.Commands.CancelQuote;
using Obss.CRM.Application.Commands.AddQuoteItem;
using Obss.CRM.Application.Commands.UpdateQuoteItem;
using Obss.CRM.Application.Commands.RemoveQuoteItem;
using Obss.CRM.Application.Queries.GetQuoteById;
using Obss.CRM.Application.Queries.GetQuotes;
using Obss.CRM.Application.Queries.GetQuotesByCustomer;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Api.Endpoints;

public static class QuoteEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/quotes", async (CreateQuoteCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/crm/quotes/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/quotes/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetQuoteByIdQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        });

        group.MapGet("/quotes", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetQuotesQuery());
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/customers/{customerId:guid}/quotes", async (Guid customerId, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetQuotesByCustomerQuery(customerId));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPatch("/quotes/{id:guid}", async (Guid id, UpdateQuoteCommand command, IMediator mediator) =>
        {
            if (id != command.Id)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/quotes/{id:guid}/submit", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new SubmitQuoteCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/quotes/{id:guid}/approve", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new ApproveQuoteCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/quotes/{id:guid}/accept", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new AcceptQuoteCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/quotes/{id:guid}/reject", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new RejectQuoteCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/quotes/{id:guid}/cancel", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new CancelQuoteCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/quotes/{id:guid}/items", async (Guid id, AddQuoteItemCommand command, IMediator mediator) =>
        {
            if (id != command.QuoteId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPatch("/quotes/{id:guid}/items/{itemId:guid}", async (Guid id, Guid itemId, UpdateQuoteItemCommand command, IMediator mediator) =>
        {
            if (id != command.QuoteId || itemId != command.ItemId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapDelete("/quotes/{id:guid}/items/{itemId:guid}", async (Guid id, Guid itemId, IMediator mediator) =>
        {
            var result = await mediator.Send(new RemoveQuoteItemCommand(id, itemId));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });
    }
}
