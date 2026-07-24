using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.ProductCatalog.Application.Commands.CreateOffer;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Infrastructure;
using Obss.ProductCatalog.Application.Commands.DeleteOffer;
using Obss.ProductCatalog.Application.Commands.PatchOffer;
using Obss.ProductCatalog.Application.Commands.UpdateOffer;
using Obss.ProductCatalog.Application.Commands.UpdateOfferPricing;
using Obss.ProductCatalog.Application.Commands.AddProductOfferingTerm;
using Obss.ProductCatalog.Application.Commands.UpdateProductOfferingTerm;
using Obss.ProductCatalog.Application.Commands.RemoveProductOfferingTerm;
using Obss.ProductCatalog.Application.Queries.GetActiveOffers;
using Obss.ProductCatalog.Application.Queries.GetOfferById;
using Obss.ProductCatalog.Application.Commands.AddBundledProductOffering;
using Obss.ProductCatalog.Application.Commands.UpdateBundledProductOffering;
using Obss.ProductCatalog.Application.Commands.RemoveBundledProductOffering;
using Obss.ProductCatalog.Application.Queries.GetBundledProductOfferings;
using Obss.ProductCatalog.Application.Queries.GetProductOfferingTerms;
using Obss.ProductCatalog.Application.Commands.AddPriceRange;
using Obss.ProductCatalog.Application.Commands.UpdatePriceRange;
using Obss.ProductCatalog.Application.Commands.RemovePriceRange;
using Obss.ProductCatalog.Application.Queries.GetPriceRanges;
using Obss.SharedKernel.Application.Authorization;

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
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Catalog.OfferWrite));

        group.MapGet("/offers/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetOfferByIdQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Catalog.OfferRead));

        group.MapGet("/offers", async ([AsParameters] GetActiveOffersQuery query, IMediator mediator, HttpContext httpContext) =>
        {
            var result = await mediator.Send(query);
            if (!result.IsSuccess)
                return (IResult)TypedResults.BadRequest(result.Error);

            var paginationRequest = new TmfPaginationRequest { Offset = query.Offset, Limit = query.Limit };
            httpContext.Response.SetPaginationHeaders(paginationRequest, result.Value.TotalCount);
            return (IResult)TypedResults.Ok(result.Value.Items);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Catalog.OfferRead));

        group.MapPut("/offers/{offerId:guid}/pricing/{offerPricingId:guid}", async (Guid offerId, Guid offerPricingId, UpdateOfferPricingCommand command, IMediator mediator) =>
        {
            if (offerId != command.OfferId || offerPricingId != command.OfferPricingId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Catalog.OfferWrite));

        group.MapPut("/offers/{id:guid}", async (Guid id, UpdateOfferCommand command, IMediator mediator) =>
        {
            if (id != command.OfferId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Catalog.OfferWrite));

        group.MapPatch("/offers/{id:guid}", async (Guid id, PatchOfferCommand command, IMediator mediator) =>
        {
            if (id != command.OfferId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Catalog.OfferWrite));

        group.MapDelete("/offers/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new DeleteOfferCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.NotFound(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Catalog.OfferWrite));

        group.MapGet("/offers/{offerId:guid}/terms", async (Guid offerId, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetProductOfferingTermsQuery(offerId));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Catalog.OfferRead));

        group.MapPost("/offers/{offerId:guid}/terms", async (Guid offerId, AddProductOfferingTermCommand command, IMediator mediator) =>
        {
            if (offerId != command.OfferId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/catalog/offers/{offerId}/terms/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Catalog.OfferWrite));

        group.MapPut("/offers/{offerId:guid}/terms/{termId:guid}", async (Guid offerId, Guid termId, UpdateProductOfferingTermCommand command, IMediator mediator) =>
        {
            if (offerId != command.OfferId || termId != command.TermId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Catalog.OfferWrite));

        group.MapDelete("/offers/{offerId:guid}/terms/{termId:guid}", async (Guid offerId, Guid termId, IMediator mediator) =>
        {
            var result = await mediator.Send(new RemoveProductOfferingTermCommand(offerId, termId));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Catalog.OfferWrite));

        group.MapGet("/offers/{offerId:guid}/bundled-offerings", async (Guid offerId, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetBundledProductOfferingsQuery(offerId));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Catalog.OfferRead));

        group.MapPost("/offers/{offerId:guid}/bundled-offerings", async (Guid offerId, AddBundledProductOfferingCommand command, IMediator mediator) =>
        {
            if (offerId != command.OfferId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/catalog/offers/{offerId}/bundled-offerings/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Catalog.OfferWrite));

        group.MapPut("/offers/{offerId:guid}/bundled-offerings/{id:guid}", async (Guid offerId, Guid id, UpdateBundledProductOfferingCommand command, IMediator mediator) =>
        {
            if (offerId != command.OfferId || id != command.BundledOfferingId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Catalog.OfferWrite));

        group.MapDelete("/offers/{offerId:guid}/bundled-offerings/{id:guid}", async (Guid offerId, Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new RemoveBundledProductOfferingCommand(offerId, id));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Catalog.OfferWrite));

        group.MapPost("/offers/{offerId:guid}/pricing/{pricingId:guid}/price-ranges", async (Guid offerId, Guid pricingId, AddPriceRangeCommand command, IMediator mediator) =>
        {
            if (pricingId != command.OfferPricingId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/catalog/offers/{offerId}/pricing/{pricingId}/price-ranges/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Catalog.OfferWrite));

        group.MapGet("/offers/{offerId:guid}/pricing/{pricingId:guid}/price-ranges", async (Guid offerId, Guid pricingId, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetPriceRangesQuery(pricingId));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Catalog.OfferRead));

        group.MapPut("/offers/{offerId:guid}/pricing/{pricingId:guid}/price-ranges/{rangeId:guid}", async (Guid offerId, Guid pricingId, Guid rangeId, UpdatePriceRangeCommand command, IMediator mediator) =>
        {
            if (pricingId != command.OfferPricingId || rangeId != command.PriceRangeId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Catalog.OfferWrite));

        group.MapDelete("/offers/{offerId:guid}/pricing/{pricingId:guid}/price-ranges/{rangeId:guid}", async (Guid offerId, Guid pricingId, Guid rangeId, IMediator mediator) =>
        {
            var result = await mediator.Send(new RemovePriceRangeCommand(pricingId, rangeId));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Catalog.OfferWrite));
    }
}
