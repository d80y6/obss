using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.Rating.Application.Commands.ApplyPromotion;
using Obss.Rating.Application.Commands.CreatePromotion;
using Obss.Rating.Application.Commands.CreateRatingRule;
using Obss.Rating.Application.Commands.DeactivatePromotion;
using Obss.Rating.Application.Commands.RateUsage;
using Obss.Rating.Application.Commands.RateUsageRealTime;
using Obss.Rating.Application.Commands.SubmitUsage;
using Obss.Rating.Application.Queries.GetApplicablePromotions;
using Obss.Rating.Application.Queries.GetPromotions;
using Obss.Rating.Application.Queries.GetRules;
using Obss.Rating.Application.Queries.GetUnratedRecords;
using Obss.Rating.Application.Queries.GetUsageBySubscription;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Infrastructure;

namespace Obss.Rating.Api.Endpoints;

public static class RatingEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/usage", async (SubmitUsageCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/rating/usage/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/usage/rate", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new RateUsageCommand());
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(new { Rated = result.Value })
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/usage/{id:guid}/rate-realtime", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new RateUsageRealTimeCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/usage/unrated", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetUnratedRecordsQuery());
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/usage/subscription/{subscriptionId:guid}", async (
            Guid subscriptionId,
            DateTime? from,
            DateTime? to,
            IMediator mediator,
            HttpContext httpContext,
            [AsParameters] TmfPaginationRequest pagination) =>
        {
            var query = new GetUsageBySubscriptionQuery(subscriptionId, from, to, pagination.Offset, pagination.Limit);
            var result = await mediator.Send(query);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/rules", async (CreateRatingRuleCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/rating/rules/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/rules", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetRulesQuery());
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/promotions", async (CreatePromotionCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/rating/promotions/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/promotions", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetPromotionsQuery());
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/promotions/{id:guid}/deactivate", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new DeactivatePromotionCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok()
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/promotions/apply", async (ApplyPromotionCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/promotions/applicable", async (
            decimal? amount,
            int? quantity,
            Guid? productId,
            Guid? subscriptionId,
            IMediator mediator) =>
        {
            var query = new GetApplicablePromotionsQuery(amount ?? 0, quantity ?? 0, productId, subscriptionId);
            var result = await mediator.Send(query);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });
    }
}
