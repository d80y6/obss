using System.Net;
using Serilog;
using Obss.SharedKernel.Domain.Exceptions;

namespace Obss.Host.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            Log.Warning(ex, "Validation failed");
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Validation failed",
                details = ex.Message
            });
        }
        catch (NotFoundException ex)
        {
            Log.Warning(ex, "Resource not found");
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Resource not found",
                details = ex.Message
            });
        }
        catch (UnauthorizedException ex)
        {
            Log.Warning(ex, "Unauthorized");
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Unauthorized",
                details = ex.Message
            });
        }
        catch (ConflictException ex)
        {
            Log.Warning(ex, "Conflict");
            context.Response.StatusCode = (int)HttpStatusCode.Conflict;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Conflict",
                details = ex.Message
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unhandled exception");
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "An unexpected error occurred",
                details = ex.Message
            });
        }
    }
}