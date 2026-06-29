namespace Obss.NetworkInventory.Application.DTOs;

public sealed record SubnetDto(
    Guid Id,
    string TenantId,
    string Network,
    string Name,
    string? Description,
    string? Gateway,
    int VLANId,
    string? Location,
    string Status,
    DateTime CreatedAt);
