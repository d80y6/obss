using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using Obss.ApiGateway.Domain.Services;

namespace Obss.Host.Middleware;

public sealed class ApiKeyAuthMiddleware
{
    private readonly RequestDelegate _next;

    public ApiKeyAuthMiddleware(RequestDelegate next)
    {
        _next = next;
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
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    context.Response.ContentType = "application/json";

                    var response = new { error = "Invalid or revoked API key." };
                    await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                    return;
                }

                if (key is not null)
                {
                    var claims = new List<Claim>
                    {
                        new("client_id", key.Name),
                        new("tenant_id", key.TenantId),
                        new("auth_method", "api_key")
                    };
                    claims.AddRange(key.Permissions.Select(p => new Claim("permission", p)));

                    var identity = new ClaimsIdentity(claims, "ApiKey");
                    context.User = new ClaimsPrincipal(identity);
                }
            }
        }

        await _next(context);
    }
}
