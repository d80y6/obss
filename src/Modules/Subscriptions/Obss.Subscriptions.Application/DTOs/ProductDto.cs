namespace Obss.Subscriptions.Application.DTOs;

public sealed record ProductDto(
    Guid Id,
    Guid CustomerId,
    string? Name,
    string? Description,
    string Status,
    Guid? ProductSpecificationId,
    Guid? ProductOfferingId,
    DateTime? ActivationDate,
    DateTime? TerminationDate,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string? Href,
    List<ProductRelationshipDto> Relationships,
    List<ProductCharacteristicDto> Characteristics,
    List<ProductPriceDto> Prices,
    List<ProductTermDto> Terms,
    List<RealizingServiceDto> RealizingServices,
    List<RealizingResourceDto> RealizingResources);

public sealed record ProductRelationshipDto(Guid Id, Guid RelatedProductId, string Type);
public sealed record ProductCharacteristicDto(Guid Id, string Name, string Value, string? ValueType);
public sealed record ProductPriceDto(Guid Id, string PriceType, string Name, decimal Amount, string Currency, int? RecurringPeriod, string? RecurringPeriodUnit);
public sealed record ProductTermDto(Guid Id, string Name, int Duration, string DurationUnit, DateTime StartDate, DateTime? EndDate, string? Description);
public sealed record RealizingServiceDto(Guid Id, Guid ServiceId, string ServiceType, string Status);
public sealed record RealizingResourceDto(Guid Id, Guid ResourceId, string ResourceType, string Status);
