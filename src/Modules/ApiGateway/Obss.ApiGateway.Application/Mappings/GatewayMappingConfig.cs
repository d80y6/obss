using Mapster;
using Obss.ApiGateway.Application.DTOs;
using Obss.ApiGateway.Domain.Entities;

namespace Obss.ApiGateway.Application.Mappings;

public static class GatewayMappingConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<ApiRoute, ApiRouteDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<ApiKey, ApiKeyDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Key, src => src.Key);

        TypeAdapterConfig<Partner, PartnerDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.ApiKeys, src => src.ApiKeys.Adapt<List<ApiKeyDto>>());
    }
}
