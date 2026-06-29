using Mapster;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.Notifications.Application.Commands.MarkNotificationAsRead;
using Obss.Notifications.Application.Commands.SendNotification;
using Obss.Notifications.Application.Commands.SendNotificationFromTemplate;
using Obss.Notifications.Application.Commands.UpdateNotificationPreference;
using Obss.Notifications.Application.DTOs;
using Obss.Notifications.Application.Queries.GetNotificationById;
using Obss.Notifications.Application.Queries.GetNotifications;
using Obss.Notifications.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Notifications.Api.Endpoints;

public static class NotificationEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/notifications", async (SendNotificationCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/notifications/notifications/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/notifications/from-template", async (
            SendNotificationFromTemplateCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/notifications/notifications/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/notifications", async (
            [AsParameters] GetNotificationsQuery query, IMediator mediator) =>
        {
            var result = await mediator.Send(query);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/notifications/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetNotificationByIdQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        });

        group.MapPost("/notifications/{id:guid}/read", async (Guid id, IMediator mediator) =>
        {
            var command = new MarkNotificationAsReadCommand(id);
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok()
                : (IResult)TypedResults.NotFound(result.Error);
        });

        group.MapPost("/preferences", async (
            UpdateNotificationPreferenceCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/preferences", async (IRepository<NotificationPreference> repository) =>
        {
            var preferences = await repository.GetAllAsync();
            return (IResult)TypedResults.Ok(preferences.Adapt<IReadOnlyList<NotificationPreferenceDto>>());
        });
    }
}
