using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.SharedKernel.Application.Behaviors;

public class AuditBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentTenant _currentTenant;
    private readonly ILogger<AuditBehavior<TRequest, TResponse>> _logger;

    public AuditBehavior(
        ICurrentUser currentUser,
        ICurrentTenant currentTenant,
        ILogger<AuditBehavior<TRequest, TResponse>> logger)
    {
        _currentUser = currentUser;
        _currentTenant = currentTenant;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        _logger.LogInformation(
            "Audit: User {UserId} in Tenant {TenantId} executed {RequestName}. Request: {RequestData}",
            _currentUser.UserId,
            _currentTenant.TenantId,
            requestName,
            JsonSerializer.Serialize(request));

        return await next();
    }
}