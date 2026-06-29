namespace Obss.NetworkInventory.Application.DTOs;

public sealed record ConnectivityLinkDto(
    Guid Id,
    string TenantId,
    string Name,
    string? Description,
    Guid SourceElementId,
    Guid SourceInterfaceId,
    Guid TargetElementId,
    Guid TargetInterfaceId,
    string LinkType,
    int Bandwidth,
    string Status,
    string? Protocol,
    int LatencyMs,
    int MTU,
    DateTime CreatedAt,
    DateTime UpdatedAt);
