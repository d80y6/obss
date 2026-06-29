using Mapster;
using MediatR;
using Obss.ApiGateway.Application.Abstractions;
using Obss.ApiGateway.Application.DTOs;
using Obss.ApiGateway.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ApiGateway.Application.Commands.RegisterApiRoute;

public sealed class RegisterApiRouteCommandHandler : IRequestHandler<RegisterApiRouteCommand, Result<ApiRouteDto>>
{
    private readonly IApiRouteRepository _routeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenant _currentTenant;

    public RegisterApiRouteCommandHandler(
        IApiRouteRepository routeRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenant currentTenant)
    {
        _routeRepository = routeRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
    }

    public async Task<Result<ApiRouteDto>> Handle(RegisterApiRouteCommand request, CancellationToken cancellationToken)
    {
        var existing = await _routeRepository.GetByPathAndMethodAsync(request.Path, request.Method, cancellationToken);
        if (existing is not null)
            return Result.Failure<ApiRouteDto>(Error.Conflict($"Route '{request.Method} {request.Path}' already exists."));

        var route = ApiRoute.Create(
            _currentTenant.TenantId ?? string.Empty,
            request.Path,
            request.Method,
            request.TargetModule,
            request.TargetPath,
            request.RequireAuthentication,
            request.RequiredPermissions,
            request.RateLimitPerMinute);

        await _routeRepository.AddAsync(route, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(route.Adapt<ApiRouteDto>());
    }
}
