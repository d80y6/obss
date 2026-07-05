using MediatR;
using Obss.ProductCatalog.Application.DTOs;
using Obss.ProductCatalog.Domain.Domain.ValueObjects;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.UpdateOffer;

public sealed record UpdateOfferCommand(
    Guid OfferId,
    string Name,
    string? Description,
    OfferType OfferType,
    bool IsContract,
    int? ContractDurationMonths,
    BillingPeriod? BillingPeriod,
    bool TaxInclusive,
    int SortOrder,
    DateTime? ValidFrom,
    DateTime? ValidTo) : IRequest<Result<OfferDto>>;
