using MediatR;
using Obss.ProductCatalog.Application.DTOs;
using Obss.ProductCatalog.Domain.Domain.ValueObjects;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.CreateOffer;

public sealed record CreateOfferCommand(
    string TenantId,
    string Name,
    string? Description,
    Guid ProductId,
    OfferType OfferType,
    bool IsContract,
    int? ContractDurationMonths,
    BillingPeriod? BillingPeriod,
    bool TaxInclusive,
    int SortOrder,
    DateTime? ValidFrom,
    DateTime? ValidTo,
    List<CreateOfferPricingItem>? Pricings) : IRequest<Result<OfferDto>>;

public sealed record CreateOfferPricingItem(
    PricingType PricingType,
    string Currency,
    decimal RecurringPrice,
    decimal OneTimePrice,
    decimal UsagePrice,
    string? UnitOfMeasure,
    int? MinQuantity,
    int? MaxQuantity,
    bool IsActive);
