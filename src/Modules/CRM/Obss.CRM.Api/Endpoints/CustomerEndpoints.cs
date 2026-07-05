using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.CRM.Domain.ValueObjects;
using Obss.CRM.Application.Commands.AddCharacteristic;
using Obss.CRM.Application.Commands.AddContact;
using Obss.CRM.Application.Commands.AddCreditProfile;
using Obss.CRM.Application.Commands.AddNote;
using Obss.CRM.Application.Commands.AddRelatedParty;
using Obss.CRM.Application.Commands.AssignCustomerToSegment;
using Obss.CRM.Application.Commands.CreateCustomer;
using Obss.CRM.Application.Commands.CreateSegment;
using Obss.CRM.Application.Commands.DeleteContact;
using Obss.CRM.Application.Commands.DeleteCustomer;
using Obss.CRM.Application.Commands.PartialUpdateCustomer;
using Obss.CRM.Application.Commands.RemoveCharacteristic;
using Obss.CRM.Application.Commands.RemoveCustomerFromSegment;
using Obss.CRM.Application.Commands.RemoveRelatedParty;
using Obss.CRM.Application.Commands.SetHubOptIn;
using Obss.CRM.Application.Commands.SuspendCustomer;
using Obss.CRM.Application.Commands.AddHub;
using Obss.CRM.Application.Commands.AddContactMedium;
using Obss.CRM.Application.Commands.AddAccountRef;
using Obss.CRM.Application.Commands.AddAgreementRef;
using Obss.CRM.Application.Commands.AddPaymentMethodRef;
using Obss.CRM.Application.Commands.RemoveHub;
using Obss.CRM.Application.Commands.RemoveContactMedium;
using Obss.CRM.Application.Commands.RemoveAccountRef;
using Obss.CRM.Application.Commands.RemoveAgreementRef;
using Obss.CRM.Application.Commands.RemovePaymentMethodRef;
using Obss.CRM.Application.Commands.UpdateCustomer;
using Obss.CRM.Application.Commands.VerifyCustomerKyc;
using Obss.CRM.Application.Queries.GetCreditProfiles;
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

        group.MapGet("/customers", async ([AsParameters] SearchCustomersQuery query, IMediator mediator, HttpContext httpContext) =>
        {
            var result = await mediator.Send(query);
            if (!result.IsSuccess)
                return (IResult)TypedResults.BadRequest(result.Error);

            httpContext.Response.Headers.Append("X-Total-Count", result.Value.TotalCount.ToString());
            httpContext.Response.Headers.Append("X-Result-Count", result.Value.Items.Count.ToString());
            return (IResult)TypedResults.Ok(result.Value.Items);
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

        group.MapDelete("/customers/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new DeleteCustomerCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapDelete("/customers/{customerId:guid}/contacts/{contactId:guid}", async (Guid customerId, Guid contactId, IMediator mediator) =>
        {
            var result = await mediator.Send(new DeleteContactCommand(customerId, contactId));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPatch("/customers/{id:guid}", async (Guid id, PartialUpdateCustomerCommand command, IMediator mediator) =>
        {
            if (id != command.Id)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/customers/{id:guid}/characteristics", async (Guid id, AddCharacteristicCommand command, IMediator mediator) =>
        {
            if (id != command.CustomerId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapDelete("/customers/{id:guid}/characteristics/{key}", async (Guid id, string key, IMediator mediator) =>
        {
            var result = await mediator.Send(new RemoveCharacteristicCommand(id, key));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/customers/{id:guid}/credit-profiles", async (Guid id, AddCreditProfileCommand command, IMediator mediator) =>
        {
            if (id != command.CustomerId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/customers/{id:guid}/credit-profiles", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetCreditProfilesQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/customers/{id:guid}/related-parties", async (Guid id, AddRelatedPartyCommand command, IMediator mediator) =>
        {
            if (id != command.CustomerId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapDelete("/customers/{id:guid}/related-parties/{referredId:guid}", async (Guid id, Guid referredId, IMediator mediator) =>
        {
            var result = await mediator.Send(new RemoveRelatedPartyCommand(id, referredId));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/customers/{id:guid}/kyc-verify", async (Guid id, VerifyCustomerKycCommand command, IMediator mediator) =>
        {
            if (id != command.CustomerId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/customers/{id:guid}/hubs", async (Guid id, AddHubCommand command, IMediator mediator) =>
        {
            if (id != command.CustomerId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapDelete("/customers/{id:guid}/hubs", async (Guid id, HubType hubType, string identifier, IMediator mediator) =>
        {
            var result = await mediator.Send(new RemoveHubCommand(id, hubType, identifier));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/customers/{id:guid}/hubs/opt-in", async (Guid id, SetHubOptInCommand command, IMediator mediator) =>
        {
            if (id != command.CustomerId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/customers/{id:guid}/contact-media", async (Guid id, AddContactMediumCommand command, IMediator mediator) =>
        {
            if (id != command.CustomerId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapDelete("/customers/{id:guid}/contact-media", async (Guid id, ContactMediumType mediumType, IMediator mediator) =>
        {
            var result = await mediator.Send(new RemoveContactMediumCommand(id, mediumType));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/customers/{id:guid}/account-refs", async (Guid id, AddAccountRefCommand command, IMediator mediator) =>
        {
            if (id != command.CustomerId) return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess ? (IResult)TypedResults.NoContent() : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapDelete("/customers/{id:guid}/account-refs/{billingAccountId:guid}", async (Guid id, Guid billingAccountId, IMediator mediator) =>
        {
            var result = await mediator.Send(new RemoveAccountRefCommand(id, billingAccountId));
            return result.IsSuccess ? (IResult)TypedResults.NoContent() : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/customers/{id:guid}/agreement-refs", async (Guid id, AddAgreementRefCommand command, IMediator mediator) =>
        {
            if (id != command.CustomerId) return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess ? (IResult)TypedResults.NoContent() : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapDelete("/customers/{id:guid}/agreement-refs/{agreementId:guid}", async (Guid id, Guid agreementId, IMediator mediator) =>
        {
            var result = await mediator.Send(new RemoveAgreementRefCommand(id, agreementId));
            return result.IsSuccess ? (IResult)TypedResults.NoContent() : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/customers/{id:guid}/payment-method-refs", async (Guid id, AddPaymentMethodRefCommand command, IMediator mediator) =>
        {
            if (id != command.CustomerId) return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess ? (IResult)TypedResults.NoContent() : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapDelete("/customers/{id:guid}/payment-method-refs/{paymentMethodId:guid}", async (Guid id, Guid paymentMethodId, IMediator mediator) =>
        {
            var result = await mediator.Send(new RemovePaymentMethodRefCommand(id, paymentMethodId));
            return result.IsSuccess ? (IResult)TypedResults.NoContent() : (IResult)TypedResults.BadRequest(result.Error);
        });
    }
}
