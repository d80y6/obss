namespace Obss.NetworkInventory.Application.DTOs;

public sealed record VLANDto(
    Guid Id,
    string TenantId,
    int VLANId,
    string Name,
    string? Description,
    string? Location,
    string Status,
    DateTime CreatedAt);
