using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.Notifications.Application.Commands.CreateNotificationTemplate;
using Obss.Notifications.Application.Commands.UpdateTemplate;
using Obss.Notifications.Application.Queries.GetNotificationTemplates;
using Obss.SharedKernel.Application.Authorization;

namespace Obss.Notifications.Api.Endpoints;

public static class TemplateEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/templates", async (
            CreateNotificationTemplateCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/notifications/templates/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Notifications.TemplateManage));

        group.MapGet("/templates", async (
            [AsParameters] GetNotificationTemplatesQuery query, IMediator mediator) =>
        {
            var result = await mediator.Send(query);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Notifications.TemplateManage));

        group.MapPut("/templates/{id:guid}", async (
            Guid id, UpdateTemplateCommand command, IMediator mediator) =>
        {
            if (id != command.TemplateId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Notifications.TemplateManage));
    }
}
