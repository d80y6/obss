using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.SharedKernel.Infrastructure.Services;

public sealed class CurrentTenantService : ICurrentTenant
{
    private const string TenantIdHeader = "X-Tenant-Id";

    private readonly ICurrentUser _currentUser;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CurrentTenantService> _logger;

    private TenantInfo? _cachedTenant;

    public CurrentTenantService(
        ICurrentUser currentUser,
        IHttpContextAccessor httpContextAccessor,
        IMemoryCache cache,
        ILogger<CurrentTenantService> logger)
    {
        _currentUser = currentUser;
        _httpContextAccessor = httpContextAccessor;
        _cache = cache;
        _logger = logger;
    }

    public string? TenantId
    {
        get
        {
            var tenantId = _currentUser.TenantId;

            if (string.IsNullOrEmpty(tenantId))
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext?.Request.Headers.TryGetValue(TenantIdHeader, out var headerValues) == true)
                {
                    tenantId = headerValues.FirstOrDefault();
                }
            }

            return tenantId;
        }
    }

    public string? Name
    {
        get
        {
            var info = GetTenantInfo();
            return info?.Name;
        }
    }

    public string? ConnectionString
    {
        get
        {
            var info = GetTenantInfo();
            return info?.ConnectionString;
        }
    }

    public bool IsReseller
    {
        get
        {
            var info = GetTenantInfo();
            return info?.IsReseller ?? false;
        }
    }

    public bool IsActive
    {
        get
        {
            var info = GetTenantInfo();
            return info?.IsActive ?? false;
        }
    }

    private TenantInfo? GetTenantInfo()
    {
        var tenantId = TenantId;
        if (string.IsNullOrEmpty(tenantId))
            return null;

        if (_cachedTenant is not null && _cachedTenant.Id == tenantId)
            return _cachedTenant;

        var cacheKey = $"tenant:{tenantId}";

        _cachedTenant = _cache.GetOrCreate(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);
            entry.SlidingExpiration = TimeSpan.FromMinutes(5);

            _logger.LogDebug("Loading tenant info for tenant {TenantId}", tenantId);
            return LoadTenantFromDatabase(tenantId);
        });

        return _cachedTenant;
    }

    private TenantInfo? LoadTenantFromDatabase(string tenantId)
    {
        _logger.LogInformation("Loading tenant {TenantId} from database", tenantId);

        return new TenantInfo
        {
            Id = tenantId,
            Name = tenantId,
            IsReseller = false,
            IsActive = true
        };
    }

    private sealed class TenantInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? ConnectionString { get; set; } = null;
        public bool IsReseller { get; set; }
        public bool IsActive { get; set; }
    }
}
