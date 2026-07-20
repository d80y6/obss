using Obss.SharedKernel.Application.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Authorization;

namespace Obss.SharedKernel.Application.Behaviors;

public sealed class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IOperationResult
{
    private readonly ICurrentUser _currentUser;
    private readonly IAuthorizationService _authorizationService;

    public AuthorizationBehavior(ICurrentUser currentUser, IAuthorizationService authorizationService)
    {
        _currentUser = currentUser;
        _authorizationService = authorizationService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is IRequirePermission requirePermission)
        {
            var permission = requirePermission.RequiredPermission;

            if (!_currentUser.IsAuthenticated)
            {
                return CreateForbiddenResult("User is not authenticated.");
            }

            if (_currentUser.HasPermission(permission))
            {
                return await next();
            }

            var authResult = await _authorizationService.AuthorizeAsync(
                _currentUser.UserId is not null
                    ? new System.Security.Claims.ClaimsPrincipal(
                        new System.Security.Claims.ClaimsIdentity(new[]
                        {
                            new System.Security.Claims.Claim("sub", _currentUser.UserId)
                        }))
                    : new System.Security.Claims.ClaimsPrincipal(),
                null,
                Permissions.PolicyName(permission));

            if (!authResult.Succeeded)
            {
                return CreateForbiddenResult($"User lacks required permission: {permission}");
            }
        }

        return await next();
    }

    private static TResponse CreateForbiddenResult(string message)
    {
        var responseType = typeof(TResponse);

        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Contracts.Result<>))
        {
            var errorType = typeof(Contracts.Error);
            var forbiddenError = errorType.GetMethod("Forbidden", [typeof(string)])?.Invoke(null, [message]);

            if (forbiddenError is not null)
            {
                var failureMethod = responseType.GetMethod("Failure", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                if (failureMethod?.MakeGenericMethod(responseType.GetGenericArguments()[0]) is { } method)
                {
                    return (TResponse)method.Invoke(null, [forbiddenError])!;
                }
            }
        }

        if (responseType == typeof(Contracts.Result))
        {
            var forbiddenError = Contracts.Error.Forbidden(message);
            return (TResponse)(object)Contracts.Result.Failure(forbiddenError);
        }

        return default!;
    }
}
