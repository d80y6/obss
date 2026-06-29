using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.SharedKernel.Infrastructure.Services;

public sealed class CacheService : IDistributedCacheService
{
    private readonly IDistributedCache _cache;
    private readonly CacheOptions _options;
    private readonly ILogger<CacheService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public CacheService(
        IDistributedCache cache,
        IOptions<CacheOptions> options,
        ILogger<CacheService> logger)
    {
        _cache = cache;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var bytes = await _cache.GetAsync(key, cancellationToken);
            if (bytes is null || bytes.Length == 0)
                return default;

            var json = System.Text.Encoding.UTF8.GetString(bytes);
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get cached value for key {CacheKey}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(value, JsonOptions);
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);

            var expiry = expiration ?? TimeSpan.FromMinutes(_options.DefaultExpirationMinutes);

            await _cache.SetAsync(key, bytes, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry
            }, cancellationToken);

            _logger.LogDebug("Cached value for key {CacheKey} with expiry {Expiry}", key, expiry);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to set cached value for key {CacheKey}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _cache.RemoveAsync(key, cancellationToken);
            _logger.LogDebug("Removed cached value for key {CacheKey}", key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to remove cached value for key {CacheKey}", key);
        }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var bytes = await _cache.GetAsync(key, cancellationToken);
            return bytes is not null && bytes.Length > 0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check existence for key {CacheKey}", key);
            return false;
        }
    }

    public async Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var existing = await GetAsync<T>(key, cancellationToken);
        if (existing is not null)
            return existing;

        var value = await factory();
        if (value is not null)
        {
            await SetAsync(key, value, expiration, cancellationToken);
        }

        return value;
    }
}

public sealed class CacheOptions
{
    public const string SectionName = "Redis";

    public string InstanceName { get; set; } = "Obss";
    public int DefaultExpirationMinutes { get; set; } = 60;
}
