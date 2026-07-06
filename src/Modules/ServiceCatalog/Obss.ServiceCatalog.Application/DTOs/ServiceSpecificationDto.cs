namespace Obss.ServiceCatalog.Application.DTOs;

public sealed record ServiceSpecificationDto(
    Guid Id,
    string TenantId,
    string Name,
    string? Description,
    string? Brand,
    string? Version,
    string LifecycleStatus,
    bool IsBundle,
    DateTime? ValidFrom,
    DateTime? ValidTo,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<ServiceSpecCharacteristicDto> Characteristics,
    List<ServiceSpecRelationshipDto> Relationships
);
