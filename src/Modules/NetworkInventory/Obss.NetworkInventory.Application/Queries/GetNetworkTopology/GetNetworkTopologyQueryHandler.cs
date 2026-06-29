using Mapster;
using MediatR;
using Obss.NetworkInventory.Application.DTOs;
using Obss.NetworkInventory.Domain.Entities;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.NetworkInventory.Application.Queries.GetNetworkTopology;

public sealed class GetNetworkTopologyQueryHandler : IRequestHandler<GetNetworkTopologyQuery, Result<NetworkTopologyDto>>
{
    private readonly IRepository<NetworkElement> _elementRepository;
    private readonly IRepository<NetworkInterface> _interfaceRepository;
    private readonly IRepository<FiberCable> _fiberRepository;
    private readonly IRepository<ConnectivityLink> _linkRepository;
    private readonly IRepository<CapacityRecord> _capacityRepository;
    private readonly IRepository<Subnet> _subnetRepository;
    private readonly IRepository<NetworkElementIpAddress> _ipRepository;

    public GetNetworkTopologyQueryHandler(
        IRepository<NetworkElement> elementRepository,
        IRepository<NetworkInterface> interfaceRepository,
        IRepository<FiberCable> fiberRepository,
        IRepository<ConnectivityLink> linkRepository,
        IRepository<CapacityRecord> capacityRepository,
        IRepository<Subnet> subnetRepository,
        IRepository<NetworkElementIpAddress> ipRepository)
    {
        _elementRepository = elementRepository;
        _interfaceRepository = interfaceRepository;
        _fiberRepository = fiberRepository;
        _linkRepository = linkRepository;
        _capacityRepository = capacityRepository;
        _subnetRepository = subnetRepository;
        _ipRepository = ipRepository;
    }

    public async Task<Result<NetworkTopologyDto>> Handle(GetNetworkTopologyQuery request, CancellationToken cancellationToken)
    {
        var allElements = await _elementRepository.GetAllAsync(cancellationToken);
        var elements = allElements.Where(e => e.Status != Domain.ValueObjects.ElementStatus.Decommissioned).ToList();

        var allCapacity = await _capacityRepository.GetAllAsync(cancellationToken);
        var latestCapacity = allCapacity
            .GroupBy(c => c.ElementId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(c => c.MeasuredAt).First().UtilizationPercent);

        var nodes = elements.Select(e => new TopologyNodeDto(
            e.Id,
            e.Name,
            e.ElementType.ToString(),
            e.Status.ToString(),
            e.Location,
            latestCapacity.GetValueOrDefault(e.Id))).ToList();

        var allInterfaces = await _interfaceRepository.GetAllAsync(cancellationToken);
        var connectedInterfaces = allInterfaces.Where(i => i.ConnectedToInterfaceId.HasValue).ToList();

        var elementMap = elements.ToDictionary(e => e.Id, e => e.Name);
        var interfaceMap = allInterfaces.ToDictionary(i => i.Id, i => i.NetworkElementId);

        var edges = new List<TopologyEdgeDto>();

        foreach (var iface in connectedInterfaces.Where(i => i.ConnectedToInterfaceId.HasValue && interfaceMap.ContainsKey(i.ConnectedToInterfaceId!.Value)))
        {
            var connectedElementId = interfaceMap[iface.ConnectedToInterfaceId!.Value];
            edges.Add(new TopologyEdgeDto(
                iface.NetworkElementId,
                elementMap.GetValueOrDefault(iface.NetworkElementId, "Unknown"),
                connectedElementId,
                elementMap.GetValueOrDefault(connectedElementId, "Unknown"),
                "Interface",
                "Active",
                iface.Speed,
                null));
        }

        var allCables = await _fiberRepository.GetAllAsync(cancellationToken);
        var activeCables = allCables.Where(c => c.Status == Domain.ValueObjects.FiberStatus.Active);

        foreach (var cable in activeCables)
        {
            edges.Add(new TopologyEdgeDto(
                cable.FromElementId,
                elementMap.GetValueOrDefault(cable.FromElementId, "Unknown"),
                cable.ToElementId,
                elementMap.GetValueOrDefault(cable.ToElementId, "Unknown"),
                "Fiber",
                cable.Status.ToString(),
                null,
                null));
        }

        var allLinks = await _linkRepository.GetAllAsync(cancellationToken);
        var activeLinks = allLinks.Where(l => l.Status != Domain.ValueObjects.LinkStatus.Down);

        foreach (var link in activeLinks)
        {
            edges.Add(new TopologyEdgeDto(
                link.SourceElementId,
                elementMap.GetValueOrDefault(link.SourceElementId, "Unknown"),
                link.TargetElementId,
                elementMap.GetValueOrDefault(link.TargetElementId, "Unknown"),
                link.LinkType.ToString(),
                link.Status.ToString(),
                link.Bandwidth,
                link.Protocol));
        }

        var allSubnets = await _subnetRepository.GetAllAsync(cancellationToken);
        var allIps = await _ipRepository.GetAllAsync(cancellationToken);

        var subnets = allSubnets.Select(s =>
        {
            var subnetIps = allIps.Where(ip => ip.IPAddress.StartsWith(s.Network.Split('/')[0].Split('.')[0])).ToList();
            return new SubnetAssociationDto(
                s.Id,
                s.Network,
                s.Name,
                s.Status.ToString(),
                subnetIps.Count);
        }).ToList();

        return Result.Success(new NetworkTopologyDto(nodes, edges, subnets));
    }
}
