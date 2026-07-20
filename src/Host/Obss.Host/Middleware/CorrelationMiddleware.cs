using Serilog.Context;

namespace Obss.Host.Middleware;

public sealed class CorrelationMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue("X-Correlation-Id", out var headerValue)
            ? headerValue.FirstOrDefault()
            : null;

        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString("N");
        }

        var requestId = Guid.NewGuid().ToString("N");

        context.Items["CorrelationId"] = correlationId;
        context.Items["RequestId"] = requestId;

        context.Response.Headers["X-Correlation-Id"] = correlationId;
        context.Response.Headers["X-Request-Id"] = requestId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("RequestId", requestId))
        {
            await _next(context);
        }
    }
}
