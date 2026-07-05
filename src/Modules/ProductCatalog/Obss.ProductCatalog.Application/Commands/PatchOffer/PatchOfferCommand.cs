using MediatR;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Application.DTOs;
using Obss.ProductCatalog.Domain.Domain.ValueObjects;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.PatchOffer;

public sealed record PatchOfferCommand(
    Guid OfferId,
    Optional<string> Name,
    Optional<string?> Description,
    Optional<OfferType> OfferType,
    Optional<bool> IsContract,
    Optional<int?> ContractDurationMonths,
    Optional<BillingPeriod?> BillingPeriod,
    Optional<bool> TaxInclusive,
    Optional<int> SortOrder,
    Optional<DateTime?> ValidFrom,
    Optional<DateTime?> ValidTo) : IRequest<Result<OfferDto>>;
