namespace Obss.ServiceInventory.Application.DTOs;

public sealed record ServiceTopologyDto(
    Guid Id,
    Guid ServiceId,
    string TopologyType,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<TopologyLinkDto> Links);
