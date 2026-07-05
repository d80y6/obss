using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.Subscriptions.Application.Commands.ActivateSubscription;
using Obss.Subscriptions.Application.Commands.CancelSubscription;
using Obss.Subscriptions.Application.Commands.ChangeOffer;
using Obss.Subscriptions.Application.Commands.ChangeQuantity;
using Obss.Subscriptions.Application.Commands.CreateSubscription;
using Obss.Subscriptions.Application.Commands.ExtendEndDate;
using Obss.Subscriptions.Application.Commands.OverrideEntitlementLimit;
using Obss.Subscriptions.Application.Commands.RenewSubscription;
using Obss.Subscriptions.Application.Commands.SetSubscriptionEntitlements;
using Obss.Subscriptions.Application.Commands.SuspendSubscription;
using Obss.Subscriptions.Application.Commands.UpdateEntitlementUsage;
using Obss.Subscriptions.Application.Queries.CheckEntitlementAvailability;
using Obss.Subscriptions.Application.Queries.GetActiveSubscriptionsByCustomer;
using Obss.Subscriptions.Application.Queries.GetEntitlementUsage;
using Obss.Subscriptions.Application.Queries.GetSubscriptionById;
using Obss.Subscriptions.Application.Queries.GetSubscriptionEntitlements;
using Obss.Subscriptions.Application.Queries.GetSubscriptions;

namespace Obss.Subscriptions.Api.Endpoints;

public static class SubscriptionEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/subscriptions", async (CreateSubscriptionCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/subscriptions/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/subscriptions/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetSubscriptionByIdQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        });

        group.MapGet("/subscriptions", async ([AsParameters] GetSubscriptionsQuery query, IMediator mediator, HttpContext httpContext) =>
        {
            var result = await mediator.Send(query);
            if (!result.IsSuccess)
                return (IResult)TypedResults.BadRequest(result.Error);

            httpContext.Response.Headers.Append("X-Total-Count", result.Value.TotalCount.ToString());
            httpContext.Response.Headers.Append("X-Result-Count", result.Value.Items.Count.ToString());
            return (IResult)TypedResults.Ok(result.Value.Items);
        });

        group.MapGet("/customers/{customerId:guid}/subscriptions", async (Guid customerId, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetActiveSubscriptionsByCustomerQuery(customerId));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/subscriptions/{id:guid}/activate", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new ActivateSubscriptionCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/subscriptions/{id:guid}/suspend", async (Guid id, SuspendSubscriptionCommand command, IMediator mediator) =>
        {
            if (id != command.SubscriptionId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/subscriptions/{id:guid}/resume", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new ActivateSubscriptionCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/subscriptions/{id:guid}/cancel", async (Guid id, CancelSubscriptionCommand command, IMediator mediator) =>
        {
            if (id != command.SubscriptionId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPut("/subscriptions/{id:guid}/offer", async (Guid id, ChangeOfferCommand command, IMediator mediator) =>
        {
            if (id != command.SubscriptionId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/subscriptions/{id:guid}/renew", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new RenewSubscriptionCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/subscriptions/{id:guid}/entitlements", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetSubscriptionEntitlementsQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        });

        group.MapPost("/subscriptions/{id:guid}/entitlements", async (Guid id, SetSubscriptionEntitlementsCommand command, IMediator mediator) =>
        {
            if (id != command.SubscriptionId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/subscriptions/{id:guid}/entitlements/usage", async (Guid id, UpdateEntitlementUsageCommand command, IMediator mediator) =>
        {
            if (id != command.SubscriptionId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/subscriptions/{id:guid}/entitlements/override", async (Guid id, OverrideEntitlementLimitCommand command, IMediator mediator) =>
        {
            if (id != command.SubscriptionId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/subscriptions/{id:guid}/entitlements/check", async (Guid id, string type, decimal amount, IMediator mediator) =>
        {
            var result = await mediator.Send(new CheckEntitlementAvailabilityQuery(id, type, amount));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/subscriptions/{id:guid}/entitlements/usage", async (Guid id, string type, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetEntitlementUsageQuery(id, type));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        });

        group.MapPut("/subscriptions/{id:guid}/quantity", async (Guid id, ChangeQuantityCommand command, IMediator mediator) =>
        {
            if (id != command.SubscriptionId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPut("/subscriptions/{id:guid}/end-date", async (Guid id, ExtendEndDateCommand command, IMediator mediator) =>
        {
            if (id != command.SubscriptionId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });
    }
}
