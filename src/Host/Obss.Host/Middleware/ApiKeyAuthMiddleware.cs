using System.Collections.Concurrent;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Obss.ApiGateway.Domain.Services;
using Obss.ApiGateway.Domain.Entities;

namespace Obss.Host.Middleware;

public sealed class ApiKeyAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _rateLimitCache;
    private readonly ILogger<ApiKeyAuthMiddleware> _logger;
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _throttles = new();

    private static readonly Dictionary<string, HashSet<string>> _permissionPathMappings = new(StringComparer.OrdinalIgnoreCase)
    {
        ["telecom.service.read"] = new(StringComparer.OrdinalIgnoreCase) { "/api/telecom/services", "/api/telecom/services/" },
        ["telecom.service.write"] = new(StringComparer.OrdinalIgnoreCase) { "/api/telecom/services", "/api/telecom/services/" },
        ["telecom.service.qualify"] = new(StringComparer.OrdinalIgnoreCase) { "/api/qualification", "/api/qualification/" },
        ["telecom.ftth.read"] = new(StringComparer.OrdinalIgnoreCase) { "/api/ftth", "/api/ftth/" },
        ["telecom.ftth.write"] = new(StringComparer.OrdinalIgnoreCase) { "/api/ftth", "/api/ftth/" },
        ["telecom.adsl.read"] = new(StringComparer.OrdinalIgnoreCase) { "/api/adsl", "/api/adsl/" },
        ["telecom.adsl.write"] = new(StringComparer.OrdinalIgnoreCase) { "/api/adsl", "/api/adsl/" },
        ["telecom.lte.read"] = new(StringComparer.OrdinalIgnoreCase) { "/api/lte", "/api/lte/" },
        ["telecom.lte.write"] = new(StringComparer.OrdinalIgnoreCase) { "/api/lte", "/api/lte/" },
        ["telecom.telephony.read"] = new(StringComparer.OrdinalIgnoreCase) { "/api/telephony", "/api/telephony/" },
        ["telecom.telephony.write"] = new(StringComparer.OrdinalIgnoreCase) { "/api/telephony", "/api/telephony/" },
        ["telecom.bundle.read"] = new(StringComparer.OrdinalIgnoreCase) { "/api/bundles", "/api/bundles/" },
        ["telecom.bundle.write"] = new(StringComparer.OrdinalIgnoreCase) { "/api/bundles", "/api/bundles/" },
        ["telecom.adapter.manage"] = new(StringComparer.OrdinalIgnoreCase) { "/api/adapters", "/api/adapters/" },
        ["telecom.adapter.read"] = new(StringComparer.OrdinalIgnoreCase) { "/api/adapters", "/api/adapters/" },
        ["telecom.usage.read"] = new(StringComparer.OrdinalIgnoreCase) { "/api/usage", "/api/usage/" },
        ["telecom.cdr.ingest"] = new(StringComparer.OrdinalIgnoreCase) { "/api/cdr", "/api/cdr/" },
        ["telecom.alarm.read"] = new(StringComparer.OrdinalIgnoreCase) { "/api/alarms", "/api/alarms/" },
        ["telecom.alarm.acknowledge"] = new(StringComparer.OrdinalIgnoreCase) { "/api/alarms", "/api/alarms/" },
        ["telecom.performance.read"] = new(StringComparer.OrdinalIgnoreCase) { "/api/performance", "/api/performance/" },
        ["billing:bills:read"] = new(StringComparer.OrdinalIgnoreCase) { "/api/billing", "/api/billing/" },
        ["billing:bills:write"] = new(StringComparer.OrdinalIgnoreCase) { "/api/billing", "/api/billing/" },
        ["payments:read"] = new(StringComparer.OrdinalIgnoreCase) { "/api/payments", "/api/payments/" },
        ["payments:write"] = new(StringComparer.OrdinalIgnoreCase) { "/api/payments", "/api/payments/" },
        ["orders:read"] = new(StringComparer.OrdinalIgnoreCase) { "/api/orders", "/api/orders/" },
        ["orders:write"] = new(StringComparer.OrdinalIgnoreCase) { "/api/orders", "/api/orders/" },
        ["catalog:products:read"] = new(StringComparer.OrdinalIgnoreCase) { "/api/catalog", "/api/catalog/" },
        ["catalog:products:write"] = new(StringComparer.OrdinalIgnoreCase) { "/api/catalog", "/api/catalog/" },
    };

    private static readonly HashSet<string> _internalPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/cdr", "/api/cdr/ingest", "/api/internal"
    };

    public ApiKeyAuthMiddleware(RequestDelegate next, IMemoryCache rateLimitCache, ILogger<ApiKeyAuthMiddleware> logger)
    {
        _next = next;
        _rateLimitCache = rateLimitCache;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IApiKeyValidator apiKeyValidator)
    {
        if (context.Request.Headers.TryGetValue("X-Api-Key", out var apiKeyHeader))
        {
            var apiKey = apiKeyHeader.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                var (valid, key) = apiKeyValidator.ValidateApiKey(apiKey);
                if (!valid)
                {
                    _logger.LogWarning("API key authentication failed: key invalid or revoked");
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    context.Response.ContentType = "application/json";

                    var response = new { error = "Invalid or revoked API key." };
                    await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                    return;
                }

                if (key is not null)
                {
                    if (key.AllowedIPs.Count > 0)
                    {
                        var remoteIp = context.Connection.RemoteIpAddress;
                        if (remoteIp is null || !IsIpAllowed(remoteIp, key.AllowedIPs))
                        {
                            _logger.LogWarning("API key {KeyName} rejected: IP {Ip} not in allowed list", key.Name, remoteIp);
                            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                            context.Response.ContentType = "application/json";

                            var response = new { error = "IP address not allowed for this API key." };
                            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                            return;
                        }
                    }

                    var requestPath = context.Request.Path.Value ?? "";
                    if (!IsPathAllowedForKey(key, requestPath))
                    {
                        _logger.LogWarning(
                            "API key {KeyName} rejected: path {Path} not in allowed scope",
                            key.Name, requestPath);
                        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        context.Response.ContentType = "application/json";

                        var response = new { error = "API key does not have permission to access this path." };
                        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                        return;
                    }

                    if (!CheckRateLimit(key, context.Request.Path))
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                        context.Response.Headers["Retry-After"] = "60";
                        context.Response.ContentType = "application/json";

                        var response = new { error = "API key rate limit exceeded." };
                        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                        return;
                    }

                    var claims = new List<Claim>
                    {
                        new("client_id", key.Name),
                        new("tenant_id", key.TenantId),
                        new("auth_method", "api_key"),
                        new("api_key_id", key.Id.ToString()),
                    };
                    claims.AddRange(key.Permissions.Select(p => new Claim("permission", p)));

                    var identity = new ClaimsIdentity(claims, "ApiKey");
                    context.User = new ClaimsPrincipal(identity);

                    _logger.LogInformation(
                        "API key used: Name={KeyName}, Tenant={Tenant}, Path={Path}, Method={Method}",
                        key.Name, key.TenantId, context.Request.Path, context.Request.Method);
                }
            }
        }

        await _next(context);
    }

    private static bool IsPathAllowedForKey(ApiKey key, string requestPath)
    {
        if (key.Permissions.Count == 0)
            return false;

        if (_internalPaths.Any(p => requestPath.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
            return key.Permissions.Contains("telecom.cdr.ingest") || key.Permissions.Contains("telecom.adapter.manage");

        foreach (var permission in key.Permissions)
        {
            if (_permissionPathMappings.TryGetValue(permission, out var allowedPaths) &&
                allowedPaths.Any(p => requestPath.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
                return true;
        }

        return false;
    }

    private bool CheckRateLimit(ApiKey key, PathString path)
    {
        if (key.RateLimitPerMinute <= 0)
            return true;

        var cacheKey = $"apikey_rate_{key.Id}_{path}";
        var throttle = _throttles.GetOrAdd(cacheKey, _ => new SemaphoreSlim(1, 1));

        if (!throttle.Wait(0))
            return true;

        try
        {
            var counter = _rateLimitCache.GetOrCreate(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
                return new RateLimitCounter { Count = 0, ResetTime = DateTime.UtcNow.AddMinutes(1) };
            });

            if (counter is null)
                return true;

            if (counter.Count >= key.RateLimitPerMinute)
            {
                if (counter.ResetTime > DateTime.UtcNow)
                    return false;

                counter.Count = 0;
                counter.ResetTime = DateTime.UtcNow.AddMinutes(1);
            }

            counter.Count++;
            return true;
        }
        finally
        {
            throttle.Release();
        }
    }

    private static bool IsIpAllowed(IPAddress remoteIp, List<string> allowedIPs)
    {
        foreach (var entry in allowedIPs)
        {
            if (entry.Contains('/'))
            {
                var parts = entry.Split('/');
                if (IPAddress.TryParse(parts[0], out var networkAddress) &&
                    int.TryParse(parts[1], out var prefixLength) &&
                    IsInCidrRange(remoteIp, networkAddress, prefixLength))
                {
                    return true;
                }
            }
            else
            {
                if (IPAddress.TryParse(entry, out var allowedIp) && remoteIp.Equals(allowedIp))
                    return true;
            }
        }

        return false;
    }

    private static bool IsInCidrRange(IPAddress address, IPAddress networkAddress, int prefixLength)
    {
        if (address.AddressFamily != networkAddress.AddressFamily)
            return false;

        var addressBytes = address.GetAddressBytes();
        var networkBytes = networkAddress.GetAddressBytes();

        var fullBytes = prefixLength / 8;
        var remainingBits = prefixLength % 8;

        for (var i = 0; i < fullBytes; i++)
        {
            if (addressBytes[i] != networkBytes[i])
                return false;
        }

        if (remainingBits > 0)
        {
            var mask = (byte)(0xFF << (8 - remainingBits));
            if ((addressBytes[fullBytes] & mask) != (networkBytes[fullBytes] & mask))
                return false;
        }

        return true;
    }

    private sealed class RateLimitCounter
    {
        public int Count { get; set; }
        public DateTime ResetTime { get; set; }
    }
}
