namespace Obss.ServiceInventory.Application.DTOs;

public sealed record ServiceResourceDto(
    Guid Id,
    Guid ServiceId,
    string ResourceType,
    string ResourceIdentifier,
    string Status,
    DateTime AllocatedAt,
    DateTime? ReleasedAt);
