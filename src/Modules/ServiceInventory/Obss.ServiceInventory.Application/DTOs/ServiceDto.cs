namespace Obss.ServiceInventory.Application.DTOs;

public sealed record ServiceDto(
    Guid Id,
    Guid TenantId,
    Guid CustomerId,
    Guid SubscriptionId,
    string ServiceType,
    string ServiceIdentifier,
    string Status,
    DateTime? ActivationDate,
    DateTime? SuspendedAt,
    DateTime? DecommissionedAt,
    string? Configuration,
    string? Location,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<ServiceResourceDto> Resources);
