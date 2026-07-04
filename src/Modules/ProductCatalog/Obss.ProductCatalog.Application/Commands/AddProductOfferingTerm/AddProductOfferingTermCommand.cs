using MediatR;
using Obss.ProductCatalog.Application.DTOs;
using Obss.ProductCatalog.Domain.Domain.ValueObjects;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.AddProductOfferingTerm;

public sealed record AddProductOfferingTermCommand(
    Guid OfferId,
    string Name,
    string? Description,
    int Duration,
    DurationUnit DurationUnit,
    TermType TermType,
    DateTime? ValidFrom,
    DateTime? ValidTo) : IRequest<Result<ProductOfferingTermDto>>;
