using System.Net;
using System.Text.Json;
using Obss.ApiGateway.Domain.Services;
using Obss.ApiGateway.Domain.ValueObjects;

namespace Obss.Host.Middleware;

public sealed class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IRateLimiter _rateLimiter;

    public RateLimitingMiddleware(RequestDelegate next, IRateLimiter rateLimiter)
    {
        _next = next;
        _rateLimiter = rateLimiter;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var apiKey = context.Request.Headers["X-Api-Key"].FirstOrDefault();
        var path = context.Request.Path.Value ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            var result = _rateLimiter.CheckRateLimit(apiKey, path);

            context.Response.Headers["X-RateLimit-Limit"] = result.Limit.ToString();
            context.Response.Headers["X-RateLimit-Remaining"] = result.RemainingRequests.ToString();

            if (!result.IsAllowed)
            {
                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                context.Response.Headers["Retry-After"] = result.RetryAfter?.TotalSeconds.ToString("F0");

                var response = new
                {
                    error = "Rate limit exceeded.",
                    retryAfter = result.RetryAfter?.TotalSeconds
                };

                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                return;
            }
        }

        await _next(context);
    }
}
