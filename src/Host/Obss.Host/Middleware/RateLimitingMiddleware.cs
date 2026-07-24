using System.Globalization;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace Obss.Host.Middleware;

public sealed class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IDistributedCache _cache;
    private readonly RateLimitingConfiguration _config;

    public RateLimitingMiddleware(RequestDelegate next, IDistributedCache cache, RateLimitingConfiguration config)
    {
        _next = next;
        _cache = cache;
        _config = config;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!_config.Enabled)
        {
            await _next(context);
            return;
        }

        var clientId = GetClientId(context);
        var path = context.Request.Path.Value ?? string.Empty;
        var limit = GetLimit(path);
        var windowKey = $"ratelimit:{clientId}:{path}:{DateTime.UtcNow:yyyyMMddHHmm}";

        var countStr = await _cache.GetStringAsync(windowKey, context.RequestAborted);
        var count = countStr is not null ? int.Parse(countStr, CultureInfo.InvariantCulture) : 0;
        count++;

        var expiry = TimeSpan.FromSeconds(60);

        if (count > limit)
        {
            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.Headers["X-RateLimit-Limit"] = limit.ToString(CultureInfo.InvariantCulture);
            context.Response.Headers["X-RateLimit-Remaining"] = "0";
            context.Response.Headers["Retry-After"] = "60";
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(
                JsonSerializer.Serialize(new { error = "Rate limit exceeded. Please try again in 60 seconds." }),
                context.RequestAborted);
            return;
        }

        await _cache.SetStringAsync(windowKey, count.ToString(CultureInfo.InvariantCulture), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiry
        }, context.RequestAborted);

        context.Response.Headers["X-RateLimit-Limit"] = limit.ToString(CultureInfo.InvariantCulture);
        context.Response.Headers["X-RateLimit-Remaining"] = (limit - count).ToString(CultureInfo.InvariantCulture);

        await _next(context);
    }

    private static string GetClientId(HttpContext context)
    {
        var apiKey = context.Request.Headers["X-Api-Key"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(apiKey))
            return apiKey;

        var sub = context.User?.FindFirst("sub")?.Value;
        if (!string.IsNullOrWhiteSpace(sub))
            return sub;

        var username = context.User?.FindFirst("preferred_username")?.Value;
        if (!string.IsNullOrWhiteSpace(username))
            return username;

        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(forwardedFor))
            return forwardedFor;

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private int GetLimit(string path)
    {
        return _config.Rules
            .Where(kvp => path.StartsWith(kvp.Key, StringComparison.OrdinalIgnoreCase))
            .Select(kvp => kvp.Value.Burst)
            .FirstOrDefault(100);
    }
}
