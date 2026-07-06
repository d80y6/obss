namespace Obss.ServiceCatalog.Application.DTOs;

public sealed record ServiceCandidateDto(
    Guid Id,
    string TenantId,
    string Name,
    string? Description,
    string LifecycleStatus,
    int Version,
    DateTime? ValidFrom,
    DateTime? ValidTo,
    Guid? ServiceSpecificationId,
    string? ServiceSpecificationName,
    Guid? BaseCandidateId,
    string? FeatureSpecification,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<ServiceCategoryDto> Categories
);
