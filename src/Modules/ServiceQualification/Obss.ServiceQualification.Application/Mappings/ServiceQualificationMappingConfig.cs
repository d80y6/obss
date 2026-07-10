using Mapster;
using Obss.ServiceQualification.Application.DTOs;
using Obss.ServiceQualification.Domain.Entities;
using Obss.ServiceQualification.Domain.ValueObjects;

namespace Obss.ServiceQualification.Application.Mappings;

public static class ServiceQualificationMappingConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<GeographicAddress, GeographicAddressDto>.NewConfig()
            .Map(dest => dest, src => src);

        TypeAdapterConfig<AlternateServiceProposal, AlternateProposalDto>.NewConfig()
            .Map(dest => dest, src => src);

        TypeAdapterConfig<QualificationItem, QualificationItemDto>.NewConfig()
            .Map(dest => dest, src => src)
            .Map(dest => dest.AlternateProposals, src => src.AlternateProposals.ToList());

        TypeAdapterConfig<Domain.Entities.ServiceQualification, ServiceQualificationDto>.NewConfig()
            .Map(dest => dest, src => src)
            .Map(dest => dest.Items, src => src.Items.ToList());
    }
}
