using Mapster;
using MediatR;
using Obss.ApiGateway.Application.Abstractions;
using Obss.ApiGateway.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ApiGateway.Application.Queries.GetApiRoutes;

public sealed class GetApiRoutesQueryHandler : IRequestHandler<GetApiRoutesQuery, Result<IReadOnlyList<ApiRouteDto>>>
{
    private readonly IApiRouteRepository _routeRepository;

    public GetApiRoutesQueryHandler(IApiRouteRepository routeRepository)
    {
        _routeRepository = routeRepository;
    }

    public async Task<Result<IReadOnlyList<ApiRouteDto>>> Handle(GetApiRoutesQuery request, CancellationToken cancellationToken)
    {
        var routes = await _routeRepository.GetAllAsync(cancellationToken);
        return Result.Success(routes.Adapt<IReadOnlyList<ApiRouteDto>>());
    }
}
