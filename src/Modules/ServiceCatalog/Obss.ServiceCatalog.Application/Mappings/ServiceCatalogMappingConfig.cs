using Mapster;
using Obss.ServiceCatalog.Domain.Entities;
using Obss.ServiceCatalog.Domain.Enums;
using Obss.ServiceCatalog.Application.DTOs;

namespace Obss.ServiceCatalog.Application.Mappings;

public static class ServiceCatalogMappingConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<ServiceCategory, ServiceCategoryDto>.NewConfig()
            .Map(dest => dest.LifecycleStatus, src => src.LifecycleStatus.ToString());

        TypeAdapterConfig<ServiceCandidate, ServiceCandidateDto>.NewConfig()
            .Map(dest => dest.LifecycleStatus, src => src.LifecycleStatus.ToString());

        TypeAdapterConfig<ServiceSpecification, ServiceSpecificationDto>.NewConfig()
            .Map(dest => dest.LifecycleStatus, src => src.LifecycleStatus.ToString());

        TypeAdapterConfig<ServiceSpecCharacteristic, ServiceSpecCharacteristicDto>.NewConfig();

        TypeAdapterConfig<ServiceSpecCharValue, ServiceSpecCharValueDto>.NewConfig();

        TypeAdapterConfig<ServiceSpecRelationship, ServiceSpecRelationshipDto>.NewConfig()
            .Map(dest => dest.RelationshipType, src => src.RelationshipType.ToString());
    }
}
