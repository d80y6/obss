using Obss.SharedKernel.Domain.Common;

namespace Obss.ApiGateway.Domain.Entities;

public class ApiRoute : AggregateRoot<Guid>
{
    private ApiRoute() { }

    private ApiRoute(
        Guid id,
        string tenantId,
        string path,
        string method,
        string targetModule,
        string targetPath,
        bool requireAuthentication,
        List<string> requiredPermissions,
        int rateLimitPerMinute)
        : base(id)
    {
        TenantId = tenantId;
        Path = path;
        Method = method;
        TargetModule = targetModule;
        TargetPath = targetPath;
        RequireAuthentication = requireAuthentication;
        RequiredPermissions = requiredPermissions;
        RateLimitPerMinute = rateLimitPerMinute;
        IsActive = true;
    }

    public string TenantId { get; private set; } = string.Empty;
    public string Path { get; private set; } = string.Empty;
    public string Method { get; private set; } = string.Empty;
    public string TargetModule { get; private set; } = string.Empty;
    public string TargetPath { get; private set; } = string.Empty;
    public bool RequireAuthentication { get; private set; }
    public List<string> RequiredPermissions { get; private set; } = [];
    public int RateLimitPerMinute { get; private set; }
    public bool IsActive { get; private set; }

    public static ApiRoute Create(
        string tenantId,
        string path,
        string method,
        string targetModule,
        string targetPath,
        bool requireAuthentication,
        List<string>? requiredPermissions = null,
        int rateLimitPerMinute = 60)
    {
        return new ApiRoute(
            Guid.NewGuid(),
            tenantId,
            path,
            method,
            targetModule,
            targetPath,
            requireAuthentication,
            requiredPermissions ?? [],
            rateLimitPerMinute);
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void UpdateRateLimit(int rateLimitPerMinute)
    {
        RateLimitPerMinute = rateLimitPerMinute;
    }
}
