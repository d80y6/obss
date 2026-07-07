using System.Net;
using System.Net.Sockets;
using System.Security.Claims;
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
                    if (key.AllowedIPs.Count > 0)
                    {
                        var remoteIp = context.Connection.RemoteIpAddress;
                        if (remoteIp is null || !IsIpAllowed(remoteIp, key.AllowedIPs))
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                            context.Response.ContentType = "application/json";

                            var response = new { error = "IP address not allowed for this API key." };
                            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                            return;
                        }
                    }

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
}
