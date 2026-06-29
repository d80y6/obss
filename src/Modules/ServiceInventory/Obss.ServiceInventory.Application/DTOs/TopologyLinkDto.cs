namespace Obss.ServiceInventory.Application.DTOs;

public sealed record TopologyLinkDto(
    Guid Id,
    Guid ServiceTopologyId,
    Guid SourceServiceId,
    Guid TargetServiceId,
    string LinkType,
    string Direction,
    string? Attributes);
