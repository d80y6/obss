namespace Obss.ServiceCatalog.Application.DTOs;

public sealed record ServiceSpecRelationshipDto(
    Guid Id,
    Guid ServiceSpecificationId,
    Guid TargetSpecificationId,
    string RelationshipType,
    string? Role,
    DateTime? ValidFrom,
    DateTime? ValidTo
);
