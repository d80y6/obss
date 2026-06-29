using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.SharedKernel.Infrastructure.Services;

public sealed class CurrentUserService : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private string[]? _roles;
    private string[]? _permissions;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public string? UserId
    {
        get
        {
            var user = User;
            if (user is null)
                return null;

            return user.FindFirstValue("sub")
                   ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }

    public string? TenantId
    {
        get
        {
            var user = User;
            if (user is null)
                return null;

            return user.FindFirstValue("tenant_id")
                   ?? user.FindFirstValue("tenant");
        }
    }

    public string? Email
    {
        get
        {
            var user = User;
            if (user is null)
                return null;

            return user.FindFirstValue("email");
        }
    }

    public string[] Roles => _roles ??= ExtractRoles();

    public string[] Permissions => _permissions ??= ExtractPermissions();

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public bool IsInRole(string role)
    {
        return Array.Exists(Roles, r => r.Equals(role, StringComparison.OrdinalIgnoreCase));
    }

    public bool HasPermission(string permission)
    {
        return Array.Exists(Permissions, p => p.Equals(permission, StringComparison.OrdinalIgnoreCase));
    }

    private string[] ExtractRoles()
    {
        var user = User;
        if (user is null)
            return [];

        return user.FindAll("role")
            .Select(c => c.Value)
            .Distinct()
            .ToArray();
    }

    private string[] ExtractPermissions()
    {
        var user = User;
        if (user is null)
            return [];

        return user.FindAll("permission")
            .Select(c => c.Value)
            .Distinct()
            .ToArray();
    }
}
