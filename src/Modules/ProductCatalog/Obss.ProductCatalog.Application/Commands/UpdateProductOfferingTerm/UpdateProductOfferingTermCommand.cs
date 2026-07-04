using MediatR;
using Obss.ProductCatalog.Application.DTOs;
using Obss.ProductCatalog.Domain.Domain.ValueObjects;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.UpdateProductOfferingTerm;

public sealed record UpdateProductOfferingTermCommand(
    Guid OfferId,
    Guid TermId,
    string Name,
    string? Description,
    int Duration,
    DurationUnit DurationUnit,
    TermType TermType,
    DateTime? ValidFrom,
    DateTime? ValidTo) : IRequest<Result<ProductOfferingTermDto>>;
