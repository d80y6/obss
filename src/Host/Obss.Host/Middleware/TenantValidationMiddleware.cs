using System.Net;
using System.Text.Json;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Host.Middleware;

public sealed class TenantValidationMiddleware
{
    private readonly RequestDelegate _next;

    public TenantValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ICurrentTenant currentTenant, ICurrentUser currentUser)
    {
        if (currentUser.IsAuthenticated)
        {
            var tenantId = currentTenant.TenantId;

            if (string.IsNullOrWhiteSpace(tenantId))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                context.Response.ContentType = "application/problem+json";
                var problem = new
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                    Title = "Tenant Required",
                    Status = 403,
                    Detail = "The authenticated principal must be associated with a tenant.",
                    Instance = context.Request.Path
                };
                await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
                return;
            }

            if (!currentTenant.IsActive)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                context.Response.ContentType = "application/problem+json";
                var problem = new
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                    Title = "Inactive Tenant",
                    Status = 403,
                    Detail = $"Tenant '{tenantId}' is not active.",
                    Instance = context.Request.Path
                };
                await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
                return;
            }
        }

        await _next(context);
    }
}
