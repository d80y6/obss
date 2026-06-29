namespace Obss.NetworkInventory.Application.DTOs;

public sealed record TopologyNodeDto(
    Guid Id,
    string Name,
    string ElementType,
    string Status,
    string? Location,
    decimal? UtilizationPercent);

public sealed record TopologyEdgeDto(
    Guid FromElementId,
    string FromElementName,
    Guid ToElementId,
    string ToElementName,
    string LinkType,
    string Status,
    int? Bandwidth,
    string? Protocol);

public sealed record SubnetAssociationDto(
    Guid SubnetId,
    string Network,
    string Name,
    string Status,
    int ElementCount);

public sealed record NetworkTopologyDto(
    List<TopologyNodeDto> Nodes,
    List<TopologyEdgeDto> Edges,
    List<SubnetAssociationDto> Subnets);
