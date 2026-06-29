using MediatR;
using Obss.ApiGateway.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ApiGateway.Application.Commands.RegisterApiRoute;

public sealed record RegisterApiRouteCommand(
    string Path,
    string Method,
    string TargetModule,
    string TargetPath,
    bool RequireAuthentication,
    List<string>? RequiredPermissions,
    int RateLimitPerMinute) : IRequest<Result<ApiRouteDto>>;
