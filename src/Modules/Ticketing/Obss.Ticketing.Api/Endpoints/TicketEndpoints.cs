using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.Ticketing.Application.Commands.AddComment;
using Obss.Ticketing.Application.Commands.ApplySlaToTicket;
using Obss.Ticketing.Application.Commands.AssignTicket;
using Obss.Ticketing.Application.Commands.CloseTicket;
using Obss.Ticketing.Application.Commands.CreateSlaDefinition;
using Obss.Ticketing.Application.Commands.CreateTicket;
using Obss.Ticketing.Application.Commands.EscalateTicket;
using Obss.Ticketing.Application.Commands.ResolveTicket;
using Obss.Ticketing.Application.Queries.GetOpenTickets;
using Obss.Ticketing.Application.Queries.GetSlaBreachedTickets;
using Obss.Ticketing.Application.Queries.GetSlaDefinitions;
using Obss.Ticketing.Application.Queries.GetTicketById;
using Obss.Ticketing.Application.Queries.GetTicketSla;
using Obss.Ticketing.Application.Queries.GetTickets;

namespace Obss.Ticketing.Api.Endpoints;

public static class TicketEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/tickets", async (CreateTicketCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/ticketing/tickets/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/tickets/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetTicketByIdQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        });

        group.MapGet("/tickets", async ([AsParameters] GetTicketsQuery query, IMediator mediator) =>
        {
            var result = await mediator.Send(query);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/tickets/open", async ([AsParameters] GetOpenTicketsQuery query, IMediator mediator) =>
        {
            var result = await mediator.Send(query);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/tickets/{id:guid}/assign", async (Guid id, AssignTicketCommand command, IMediator mediator) =>
        {
            if (id != command.TicketId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/tickets/{id:guid}/comments", async (Guid id, AddCommentCommand command, IMediator mediator) =>
        {
            if (id != command.TicketId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/ticketing/tickets/{id}/comments/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/tickets/{id:guid}/resolve", async (Guid id, ResolveTicketCommand command, IMediator mediator) =>
        {
            if (id != command.TicketId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/tickets/{id:guid}/close", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new CloseTicketCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/tickets/{id:guid}/escalate", async (Guid id, EscalateTicketCommand command, IMediator mediator) =>
        {
            if (id != command.TicketId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPut("/tickets/{id:guid}/apply-sla", async (Guid id, ApplySlaToTicketCommand command, IMediator mediator) =>
        {
            if (id != command.TicketId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/tickets/{id:guid}/sla", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetTicketSlaQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        });

        group.MapPost("/sla-definitions", async (CreateSlaDefinitionCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/ticketing/sla-definitions/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/sla-definitions", async ([AsParameters] GetSlaDefinitionsQuery query, IMediator mediator) =>
        {
            var result = await mediator.Send(query);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/tickets/sla-breached", async ([AsParameters] GetSlaBreachedTicketsQuery query, IMediator mediator) =>
        {
            var result = await mediator.Send(query);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });
    }
}
