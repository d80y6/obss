namespace Obss.IAM.Application.DTOs;

public sealed record RoleDto(
    Guid Id,
    string TenantId,
    string Name,
    string? Description,
    bool IsSystem,
    DateTime CreatedAt,
    List<PermissionDto> Permissions);
