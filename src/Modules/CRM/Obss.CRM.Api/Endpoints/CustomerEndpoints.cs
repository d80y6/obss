using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.CRM.Application.Commands.AddContact;
using Obss.CRM.Application.Commands.AddNote;
using Obss.CRM.Application.Commands.AssignCustomerToSegment;
using Obss.CRM.Application.Commands.CreateCustomer;
using Obss.CRM.Application.Commands.CreateSegment;
using Obss.CRM.Application.Commands.RemoveCustomerFromSegment;
using Obss.CRM.Application.Commands.SuspendCustomer;
using Obss.CRM.Application.Commands.UpdateCustomer;
using Obss.CRM.Application.Queries.GetContactsByCustomer;
using Obss.CRM.Application.Queries.GetCustomerById;
using Obss.CRM.Application.Queries.GetCustomerSegmentAssignments;
using Obss.CRM.Application.Queries.GetCustomerSegments;
using Obss.CRM.Application.Queries.GetNotesByCustomer;
using Obss.CRM.Application.Queries.GetSegmentById;
using Obss.CRM.Application.Queries.SearchCustomers;

namespace Obss.CRM.Api.Endpoints;

public static class CustomerEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/customers", async (CreateCustomerCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/crm/customers/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/customers/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetCustomerByIdQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        });

        group.MapGet("/customers", async ([AsParameters] SearchCustomersQuery query, IMediator mediator) =>
        {
            var result = await mediator.Send(query);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPut("/customers/{id:guid}", async (Guid id, UpdateCustomerCommand command, IMediator mediator) =>
        {
            if (id != command.CustomerId)
                return (IResult)TypedResults.BadRequest();

            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/customers/{id:guid}/suspend", async (Guid id, SuspendCustomerCommand command, IMediator mediator) =>
        {
            if (id != command.CustomerId)
                return (IResult)TypedResults.BadRequest();

            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/customers/{id:guid}/contacts", async (Guid id, AddContactCommand command, IMediator mediator) =>
        {
            if (id != command.CustomerId)
                return (IResult)TypedResults.BadRequest();

            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/crm/customers/{id}/contacts/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/customers/{id:guid}/notes", async (Guid id, AddNoteCommand command, IMediator mediator) =>
        {
            if (id != command.CustomerId)
                return (IResult)TypedResults.BadRequest();

            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/crm/customers/{id}/notes/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/customers/{id:guid}/contacts", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetContactsByCustomerQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/customers/{id:guid}/notes", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetNotesByCustomerQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/segments", async (CreateSegmentCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/crm/segments/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/segments", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetCustomerSegmentsQuery());
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/segments/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetSegmentByIdQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        });

        group.MapGet("/segments/{id:guid}/assignments", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetCustomerSegmentAssignmentsQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        });

        group.MapPost("/segments/{id:guid}/assign/{customerId:guid}", async (Guid id, Guid customerId, Guid assignedBy, IMediator mediator) =>
        {
            var result = await mediator.Send(new AssignCustomerToSegmentCommand(id, customerId, assignedBy));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapDelete("/segments/{id:guid}/customers/{customerId:guid}", async (Guid id, Guid customerId, IMediator mediator) =>
        {
            var result = await mediator.Send(new RemoveCustomerFromSegmentCommand(id, customerId));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });
    }
}
