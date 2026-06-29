using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.ProductCatalog.Application.Commands.CreateOffer;
using Obss.ProductCatalog.Application.Commands.UpdateOfferPricing;
using Obss.ProductCatalog.Application.Queries.GetActiveOffers;
using Obss.ProductCatalog.Application.Queries.GetOfferById;

namespace Obss.ProductCatalog.Api.Endpoints;

public static class OfferEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/offers", async (CreateOfferCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/catalog/offers/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/offers/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetOfferByIdQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        });

        group.MapGet("/offers", async ([AsParameters] GetActiveOffersQuery query, IMediator mediator) =>
        {
            var result = await mediator.Send(query);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPut("/offers/{offerId:guid}/pricing/{offerPricingId:guid}", async (Guid offerId, Guid offerPricingId, UpdateOfferPricingCommand command, IMediator mediator) =>
        {
            if (offerId != command.OfferId || offerPricingId != command.OfferPricingId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });
    }
}
