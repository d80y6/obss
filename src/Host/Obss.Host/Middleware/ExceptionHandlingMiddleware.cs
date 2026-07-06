using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Domain.Exceptions;
using ValidationException = Obss.SharedKernel.Domain.Exceptions.ValidationException;

namespace Obss.Host.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (FluentValidation.ValidationException ex)
        {
            _logger.LogWarning(ex, "FluentValidation failed");
            var error = TmfErrorResponse.FromValidationException(ex);
            await WriteErrorResponse(context, StatusCodes.Status400BadRequest, error);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Domain validation failed");
            var error = TmfErrorResponse.Create(StatusCodes.Status400BadRequest, "VALIDATION_ERROR", "Validation failed", ex.Message);
            await WriteErrorResponse(context, StatusCodes.Status400BadRequest, error);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Resource not found");
            var error = TmfErrorResponse.Create(StatusCodes.Status404NotFound, "NOT_FOUND", "Resource not found", ex.Message);
            await WriteErrorResponse(context, StatusCodes.Status404NotFound, error);
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning(ex, "Unauthorized");
            var error = TmfErrorResponse.Create(StatusCodes.Status401Unauthorized, "UNAUTHORIZED", "Unauthorized", ex.Message);
            await WriteErrorResponse(context, StatusCodes.Status401Unauthorized, error);
        }
        catch (ConflictException ex)
        {
            _logger.LogWarning(ex, "Conflict");
            var error = TmfErrorResponse.Create(StatusCodes.Status409Conflict, "CONFLICT", "Conflict", ex.Message);
            await WriteErrorResponse(context, StatusCodes.Status409Conflict, error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            var error = TmfErrorResponse.Create(StatusCodes.Status500InternalServerError, "INTERNAL_ERROR", "An unexpected error occurred", ex.Message);
            await WriteErrorResponse(context, StatusCodes.Status500InternalServerError, error);
        }
    }

    private static async Task WriteErrorResponse(HttpContext context, int statusCode, TmfErrorResponse error)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        var json = JsonSerializer.Serialize(error, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        await context.Response.WriteAsync(json);
    }
}