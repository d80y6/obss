using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.ProductCatalog.Application.Commands.CreateOffer;
using Obss.ProductCatalog.Application.Commands.DeleteOffer;
using Obss.ProductCatalog.Application.Commands.PatchOffer;
using Obss.ProductCatalog.Application.Commands.UpdateOffer;
using Obss.ProductCatalog.Application.Commands.UpdateOfferPricing;
using Obss.ProductCatalog.Application.Commands.AddProductOfferingTerm;
using Obss.ProductCatalog.Application.Commands.UpdateProductOfferingTerm;
using Obss.ProductCatalog.Application.Commands.RemoveProductOfferingTerm;
using Obss.ProductCatalog.Application.Queries.GetActiveOffers;
using Obss.ProductCatalog.Application.Queries.GetOfferById;
using Obss.ProductCatalog.Application.Queries.GetProductOfferingTerms;

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

        group.MapGet("/offers", async ([AsParameters] GetActiveOffersQuery query, IMediator mediator, HttpContext httpContext) =>
        {
            var result = await mediator.Send(query);
            if (!result.IsSuccess)
                return (IResult)TypedResults.BadRequest(result.Error);

            httpContext.Response.Headers.Append("X-Total-Count", result.Value.TotalCount.ToString());
            httpContext.Response.Headers.Append("X-Result-Count", result.Value.Items.Count.ToString());
            return (IResult)TypedResults.Ok(result.Value.Items);
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

        group.MapPut("/offers/{id:guid}", async (Guid id, UpdateOfferCommand command, IMediator mediator) =>
        {
            if (id != command.OfferId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPatch("/offers/{id:guid}", async (Guid id, PatchOfferCommand command, IMediator mediator) =>
        {
            if (id != command.OfferId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapDelete("/offers/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new DeleteOfferCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.NotFound(result.Error);
        });

        group.MapGet("/offers/{offerId:guid}/terms", async (Guid offerId, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetProductOfferingTermsQuery(offerId));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        });

        group.MapPost("/offers/{offerId:guid}/terms", async (Guid offerId, AddProductOfferingTermCommand command, IMediator mediator) =>
        {
            if (offerId != command.OfferId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/catalog/offers/{offerId}/terms/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPut("/offers/{offerId:guid}/terms/{termId:guid}", async (Guid offerId, Guid termId, UpdateProductOfferingTermCommand command, IMediator mediator) =>
        {
            if (offerId != command.OfferId || termId != command.TermId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapDelete("/offers/{offerId:guid}/terms/{termId:guid}", async (Guid offerId, Guid termId, IMediator mediator) =>
        {
            var result = await mediator.Send(new RemoveProductOfferingTermCommand(offerId, termId));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });
    }
}
