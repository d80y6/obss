using Mapster;
using Obss.ModuleTemplate.Application.DTOs;
using Obss.ModuleTemplate.Domain.Entities;

namespace Obss.ModuleTemplate.Application.Mappings;

public static class SampleMappingConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<SampleAggregate, SampleDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.TenantId, src => src.TenantId);
    }
}
