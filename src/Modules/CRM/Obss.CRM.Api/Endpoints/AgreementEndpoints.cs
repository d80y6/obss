using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.CRM.Application.Commands.CreateAgreement;
using Obss.CRM.Application.Commands.UpdateAgreement;
using Obss.CRM.Application.Queries.GetAgreementById;
using Obss.CRM.Application.Queries.SearchAgreements;
using Obss.SharedKernel.Application.Authorization;

namespace Obss.CRM.Api.Endpoints;

public static class AgreementEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/agreements", async (CreateAgreementCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/crm/agreements/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Crm.AgreementWrite));

        group.MapGet("/agreements/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetAgreementByIdQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Crm.AgreementRead));

        group.MapPut("/agreements/{id:guid}", async (Guid id, UpdateAgreementCommand command, IMediator mediator) =>
        {
            if (id != command.Id)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Crm.AgreementWrite));

        group.MapGet("/agreements", async ([AsParameters] SearchAgreementsQuery query, IMediator mediator, HttpContext httpContext) =>
        {
            var result = await mediator.Send(query);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Crm.AgreementRead));
    }
}
