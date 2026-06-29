using Mapster;
using MediatR;
using Obss.ServiceInventory.Application.Abstractions;
using Obss.ServiceInventory.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ServiceInventory.Application.Queries.GetUnmatchedResources;

public sealed class GetUnmatchedResourcesQueryHandler : IRequestHandler<GetUnmatchedResourcesQuery, Result<List<DiscoveryJobDto>>>
{
    private readonly IResourceDiscoveryJobRepository _discoveryJobRepository;

    public GetUnmatchedResourcesQueryHandler(IResourceDiscoveryJobRepository discoveryJobRepository)
    {
        _discoveryJobRepository = discoveryJobRepository;
    }

    public async Task<Result<List<DiscoveryJobDto>>> Handle(GetUnmatchedResourcesQuery request, CancellationToken cancellationToken)
    {
        var jobs = await _discoveryJobRepository.GetUnmatchedAsync(request.TenantId, cancellationToken);
        return Result.Success(jobs.Adapt<List<DiscoveryJobDto>>());
    }
}
