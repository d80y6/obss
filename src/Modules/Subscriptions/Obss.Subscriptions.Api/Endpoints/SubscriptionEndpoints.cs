using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.Subscriptions.Application.Commands.ActivateSubscription;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Infrastructure;
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
using Obss.Subscriptions.Application.Commands.CreateProduct;
using Obss.Subscriptions.Application.Commands.UpdateProduct;
using Obss.Subscriptions.Application.Commands.ActivateProduct;
using Obss.Subscriptions.Application.Commands.SuspendProduct;
using Obss.Subscriptions.Application.Commands.CancelProduct;
using Obss.Subscriptions.Application.Queries.GetProductById;
using Obss.Subscriptions.Application.Queries.GetProducts;
using Obss.Subscriptions.Application.Queries.GetProductsByCustomer;
using Obss.SharedKernel.Application.Authorization;

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
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Subscriptions.SubscriptionWrite));

        group.MapGet("/subscriptions/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetSubscriptionByIdQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Subscriptions.SubscriptionRead));

        group.MapGet("/subscriptions", async ([AsParameters] GetSubscriptionsQuery query, IMediator mediator, HttpContext httpContext) =>
        {
            var result = await mediator.Send(query);
            if (!result.IsSuccess)
                return (IResult)TypedResults.BadRequest(result.Error);

            var paginationRequest = new TmfPaginationRequest { Offset = query.Offset, Limit = query.Limit };
            httpContext.Response.SetPaginationHeaders(paginationRequest, result.Value.TotalCount);
            return (IResult)TypedResults.Ok(result.Value.Items);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Subscriptions.SubscriptionRead));

        group.MapGet("/customers/{customerId:guid}/subscriptions", async (Guid customerId, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetActiveSubscriptionsByCustomerQuery(customerId));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Subscriptions.SubscriptionRead));

        group.MapPost("/subscriptions/{id:guid}/activate", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new ActivateSubscriptionCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Subscriptions.SubscriptionActivate));

        group.MapPost("/subscriptions/{id:guid}/suspend", async (Guid id, SuspendSubscriptionCommand command, IMediator mediator) =>
        {
            if (id != command.SubscriptionId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Subscriptions.SubscriptionSuspend));

        group.MapPost("/subscriptions/{id:guid}/resume", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new ActivateSubscriptionCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Subscriptions.SubscriptionWrite));

        group.MapPost("/subscriptions/{id:guid}/cancel", async (Guid id, CancelSubscriptionCommand command, IMediator mediator) =>
        {
            if (id != command.SubscriptionId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Subscriptions.SubscriptionCancel));

        group.MapPut("/subscriptions/{id:guid}/offer", async (Guid id, ChangeOfferCommand command, IMediator mediator) =>
        {
            if (id != command.SubscriptionId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Subscriptions.SubscriptionWrite));

        group.MapPost("/subscriptions/{id:guid}/renew", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new RenewSubscriptionCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Subscriptions.SubscriptionWrite));

        group.MapGet("/subscriptions/{id:guid}/entitlements", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetSubscriptionEntitlementsQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Subscriptions.SubscriptionRead));

        group.MapPost("/subscriptions/{id:guid}/entitlements", async (Guid id, SetSubscriptionEntitlementsCommand command, IMediator mediator) =>
        {
            if (id != command.SubscriptionId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Subscriptions.SubscriptionWrite));

        group.MapPost("/subscriptions/{id:guid}/entitlements/usage", async (Guid id, UpdateEntitlementUsageCommand command, IMediator mediator) =>
        {
            if (id != command.SubscriptionId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Subscriptions.SubscriptionWrite));

        group.MapPost("/subscriptions/{id:guid}/entitlements/override", async (Guid id, OverrideEntitlementLimitCommand command, IMediator mediator) =>
        {
            if (id != command.SubscriptionId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Subscriptions.SubscriptionWrite));

        group.MapGet("/subscriptions/{id:guid}/entitlements/check", async (Guid id, string type, decimal amount, IMediator mediator) =>
        {
            var result = await mediator.Send(new CheckEntitlementAvailabilityQuery(id, type, amount));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Subscriptions.SubscriptionRead));

        group.MapGet("/subscriptions/{id:guid}/entitlements/usage", async (Guid id, string type, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetEntitlementUsageQuery(id, type));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Subscriptions.SubscriptionRead));

        group.MapPut("/subscriptions/{id:guid}/quantity", async (Guid id, ChangeQuantityCommand command, IMediator mediator) =>
        {
            if (id != command.SubscriptionId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Subscriptions.SubscriptionWrite));

        group.MapPut("/subscriptions/{id:guid}/end-date", async (Guid id, ExtendEndDateCommand command, IMediator mediator) =>
        {
            if (id != command.SubscriptionId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Subscriptions.SubscriptionWrite));

        // Product endpoints
        group.MapPost("/products", async (CreateProductCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/subscriptions/products/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Subscriptions.ProductWrite));

        group.MapGet("/products/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetProductByIdQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Subscriptions.ProductRead));

        group.MapGet("/products", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetProductsQuery());
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Subscriptions.ProductRead));

        group.MapGet("/customers/{customerId:guid}/products", async (Guid customerId, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetProductsByCustomerQuery(customerId));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Subscriptions.ProductRead));

        group.MapPatch("/products/{id:guid}", async (Guid id, UpdateProductCommand command, IMediator mediator) =>
        {
            if (id != command.Id)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Subscriptions.ProductWrite));

        group.MapPost("/products/{id:guid}/activate", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new ActivateProductCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Subscriptions.ProductWrite));

        group.MapPost("/products/{id:guid}/suspend", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new SuspendProductCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Subscriptions.ProductWrite));

        group.MapPost("/products/{id:guid}/cancel", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new CancelProductCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Subscriptions.ProductWrite));
    }
}
