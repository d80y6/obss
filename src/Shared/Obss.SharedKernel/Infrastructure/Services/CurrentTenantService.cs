using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.SharedKernel.Infrastructure.Services;

public sealed class CurrentTenantService : ICurrentTenant
{
    private readonly ICurrentUser _currentUser;
    private readonly ITenantStore _tenantStore;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CurrentTenantService> _logger;

    private TenantInfo? _cachedTenant;

    public CurrentTenantService(
        ICurrentUser currentUser,
        ITenantStore tenantStore,
        IMemoryCache cache,
        ILogger<CurrentTenantService> logger)
    {
        _currentUser = currentUser;
        _tenantStore = tenantStore;
        _cache = cache;
        _logger = logger;
    }

    public string? TenantId => _currentUser.TenantId;

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
            return LoadTenantFromStoreAsync(tenantId).GetAwaiter().GetResult();
        });

        return _cachedTenant;
    }

    private async Task<TenantInfo?> LoadTenantFromStoreAsync(string tenantId)
    {
        _logger.LogInformation("Loading tenant {TenantId} from store", tenantId);

        var tenant = await _tenantStore.GetTenantAsync(tenantId);

        if (tenant is null)
        {
            _logger.LogWarning("Tenant {TenantId} not found in store", tenantId);
            return null;
        }

        return tenant;
    }
}
