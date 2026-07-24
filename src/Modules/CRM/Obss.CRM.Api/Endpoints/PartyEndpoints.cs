using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.CRM.Application.Commands.AddIdentityDocument;
using Obss.CRM.Application.Commands.CreateIndividual;
using Obss.CRM.Application.Commands.CreateOrganization;
using Obss.CRM.Application.Commands.RemoveIdentityDocument;
using Obss.CRM.Application.Commands.UpdateIndividual;
using Obss.CRM.Application.Commands.UpdateOrganization;
using Obss.CRM.Application.Queries.GetIndividualById;
using Obss.CRM.Application.Queries.GetOrganizationById;
using Obss.SharedKernel.Application.Authorization;

namespace Obss.CRM.Api.Endpoints;

public static class PartyEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/individuals", async (CreateIndividualCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/crm/individuals/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Customers.CustomerWrite));

        group.MapGet("/individuals/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetIndividualByIdQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Customers.CustomerRead));

        group.MapPut("/individuals/{id:guid}", async (Guid id, UpdateIndividualCommand command, IMediator mediator) =>
        {
            if (id != command.Id)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Customers.CustomerWrite));

        group.MapPost("/individuals/{individualId:guid}/documents", async (Guid individualId, AddIdentityDocumentCommand command, IMediator mediator) =>
        {
            if (individualId != command.IndividualId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Customers.CustomerWrite));

        group.MapDelete("/individuals/{individualId:guid}/documents/{docId:guid}", async (Guid individualId, Guid docId, IMediator mediator) =>
        {
            var result = await mediator.Send(new RemoveIdentityDocumentCommand(individualId, docId));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Customers.CustomerWrite));

        group.MapPost("/organizations", async (CreateOrganizationCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/crm/organizations/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Customers.CustomerWrite));

        group.MapGet("/organizations/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetOrganizationByIdQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Customers.CustomerRead));

        group.MapPut("/organizations/{id:guid}", async (Guid id, UpdateOrganizationCommand command, IMediator mediator) =>
        {
            if (id != command.Id)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Customers.CustomerWrite));
    }
}
