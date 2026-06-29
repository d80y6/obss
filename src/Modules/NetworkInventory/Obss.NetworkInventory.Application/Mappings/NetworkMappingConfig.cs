using Mapster;
using Obss.NetworkInventory.Application.DTOs;
using Obss.NetworkInventory.Application.Queries.GetTopologyMaps;
using Obss.NetworkInventory.Domain.Entities;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.NetworkInventory.Application.Mappings;

public static class NetworkMappingConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<NetworkElement, NetworkElementDto>.NewConfig()
            .Map(dest => dest.TenantId, src => src.TenantId.Value)
            .Map(dest => dest.ElementType, src => src.ElementType.ToString())
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.IPAddress, src => src.IPAddress)
            .Map(dest => dest.Hostname, src => src.Hostname)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Vendor, src => src.Vendor)
            .Map(dest => dest.Model, src => src.Model)
            .Map(dest => dest.SoftwareVersion, src => src.SoftwareVersion)
            .Map(dest => dest.SerialNumber, src => src.SerialNumber)
            .Map(dest => dest.Location, src => src.Location)
            .Map(dest => dest.ManagementIP, src => src.ManagementIP)
            .Map(dest => dest.IsManaged, src => src.IsManaged)
            .Map(dest => dest.Interfaces, src => src.Interfaces.Adapt<List<NetworkInterfaceDto>>())
            .Map(dest => dest.IpAddresses, src => src.IpAddresses.Adapt<List<NetworkIpAddressDto>>());

        TypeAdapterConfig<NetworkInterface, NetworkInterfaceDto>.NewConfig()
            .Map(dest => dest.InterfaceType, src => src.InterfaceType.ToString())
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<NetworkElementIpAddress, NetworkIpAddressDto>.NewConfig()
            .Map(dest => dest.AddressType, src => src.AddressType.ToString())
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<Subnet, SubnetDto>.NewConfig()
            .Map(dest => dest.TenantId, src => src.TenantId.Value)
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<VLAN, VLANDto>.NewConfig()
            .Map(dest => dest.TenantId, src => src.TenantId.Value)
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<OLT, OLTDetailDto>.NewConfig()
            .Map(dest => dest.TenantId, src => src.TenantId.Value)
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<PONPort, PONPortDto>.NewConfig()
            .Map(dest => dest.PortType, src => src.PortType.ToString())
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<ConnectivityLink, ConnectivityLinkDto>.NewConfig()
            .Map(dest => dest.TenantId, src => src.TenantId.Value)
            .Map(dest => dest.LinkType, src => src.LinkType.ToString())
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<CapacityRecord, CapacityRecordDto>.NewConfig()
            .Map(dest => dest.CapacityType, src => src.CapacityType.ToString())
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<TopologyMap, TopologyMapDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id);
    }
}
