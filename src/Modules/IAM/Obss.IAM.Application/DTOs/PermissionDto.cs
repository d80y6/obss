namespace Obss.IAM.Application.DTOs;

public sealed record PermissionDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    string Module,
    string Resource,
    string Action);
