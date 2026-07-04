using Obss.ProductCatalog.Domain.Domain.ValueObjects;

namespace Obss.ProductCatalog.Application.DTOs;

public sealed record ProductOfferingTermDto(
    Guid Id,
    Guid OfferId,
    string Name,
    string? Description,
    int Duration,
    DurationUnit DurationUnit,
    TermType TermType,
    DateTime? ValidFrom,
    DateTime? ValidTo);
