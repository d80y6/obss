using Mapster;
using Obss.AAA.Application.DTOs;
using Obss.AAA.Domain.Entities;

namespace Obss.AAA.Application.Mappings;

public static class AaaMappingConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<NetworkAccessServer, NasDto>.NewConfig()
            .Map(dest => dest.NasType, src => src.NasType.ToString())
            .Map(dest => dest.Status, src => src.Status)
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<RadiusSession, RadiusSessionDto>.NewConfig()
            .Map(dest => dest.SessionStatus, src => src.SessionStatus.ToString())
            .Map(dest => dest.Id, src => src.Id);
    }
}
