using MediatR;
using Obss.ProductCatalog.Application.DTOs;
using Obss.ProductCatalog.Domain.Domain.ValueObjects;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.UpdateOfferPricing;

public sealed record UpdateOfferPricingCommand(
    Guid OfferId,
    Guid OfferPricingId,
    PricingType PricingType,
    string Currency,
    decimal RecurringPrice,
    decimal OneTimePrice,
    decimal UsagePrice,
    string? UnitOfMeasure,
    int? MinQuantity,
    int? MaxQuantity,
    bool IsActive) : IRequest<Result<OfferPricingDto>>;
