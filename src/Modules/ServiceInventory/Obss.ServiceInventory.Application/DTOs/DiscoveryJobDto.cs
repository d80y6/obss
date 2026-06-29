namespace Obss.ServiceInventory.Application.DTOs;

public sealed record DiscoveryJobDto(
    Guid Id,
    Guid TenantId,
    string DiscoveryType,
    string? Configuration,
    string Status,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    int ResourcesFound,
    int ResourcesMatched,
    string? ErrorMessage,
    string CreatedBy,
    DateTime CreatedAt);
