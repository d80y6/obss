namespace Obss.SharedKernel.Application.Abstractions;

public interface ICurrentUser
{
    string? UserId { get; }
    string? TenantId { get; }
    string? Email { get; }
    string[] Roles { get; }
    string[] Permissions { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
    bool HasPermission(string permission);
}
