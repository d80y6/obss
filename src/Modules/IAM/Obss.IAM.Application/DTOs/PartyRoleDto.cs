namespace Obss.IAM.Application.DTOs;

public sealed record PartyRoleDto(
    Guid Id,
    Guid PartyId,
    Guid RoleId,
    string Name,
    string? Description,
    string Status,
    DateTime? ValidFrom,
    DateTime? ValidUntil,
    DateTime CreatedAt,
    DateTime UpdatedAt);
