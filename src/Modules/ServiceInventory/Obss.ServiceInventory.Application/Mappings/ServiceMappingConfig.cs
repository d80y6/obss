using Mapster;
using Obss.ServiceInventory.Application.DTOs;
using Obss.ServiceInventory.Domain.Entities;
using Obss.ServiceInventory.Domain.ValueObjects;

namespace Obss.ServiceInventory.Application.Mappings;

public static class ServiceMappingConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<Service, ServiceDto>.NewConfig()
            .Map(dest => dest.ServiceType, src => src.ServiceType.ToString())
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.Resources, src => src.Resources.Adapt<List<ServiceResourceDto>>());

        TypeAdapterConfig<ServiceResource, ServiceResourceDto>.NewConfig()
            .Map(dest => dest.ResourceType, src => src.ResourceType.ToString())
            .Map(dest => dest.Status, src => src.Status.ToString());

        TypeAdapterConfig<ServiceTopology, ServiceTopologyDto>.NewConfig()
            .Map(dest => dest.TopologyType, src => src.TopologyType.ToString())
            .Map(dest => dest.Links, src => src.Links.Adapt<List<TopologyLinkDto>>());

        TypeAdapterConfig<TopologyLink, TopologyLinkDto>.NewConfig()
            .Map(dest => dest.LinkType, src => src.LinkType.ToString())
            .Map(dest => dest.Direction, src => src.Direction.ToString());

        TypeAdapterConfig<ResourceDiscoveryJob, DiscoveryJobDto>.NewConfig()
            .Map(dest => dest.DiscoveryType, src => src.DiscoveryType.ToString())
            .Map(dest => dest.Status, src => src.Status.ToString());
    }
}
