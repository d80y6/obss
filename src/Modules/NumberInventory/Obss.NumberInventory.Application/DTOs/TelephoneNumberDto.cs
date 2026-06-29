namespace Obss.NumberInventory.Application.DTOs;

public sealed record TelephoneNumberDto(
    Guid Id,
    string TenantId,
    string Number,
    string NumberType,
    string Status,
    Guid? CustomerId,
    Guid? SubscriptionId,
    DateTime? AssignedAt,
    DateTime? ReservedAt,
    decimal Cost,
    string Currency,
    string? Notes,
    DateTime CreatedAt,
    DateTime UpdatedAt);
