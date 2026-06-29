namespace Obss.ApiGateway.Application.DTOs;

public sealed record ApiRouteDto(
    Guid Id,
    string TenantId,
    string Path,
    string Method,
    string TargetModule,
    string TargetPath,
    bool RequireAuthentication,
    List<string> RequiredPermissions,
    int RateLimitPerMinute,
    bool IsActive);
