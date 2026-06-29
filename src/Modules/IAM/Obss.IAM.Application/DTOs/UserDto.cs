namespace Obss.IAM.Application.DTOs;

public sealed record UserDto(
    Guid Id,
    string TenantId,
    string Username,
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    bool IsActive,
    bool EmailVerified,
    DateTime? LastLoginAt,
    string? ExternalId,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<RoleDto> Roles);
