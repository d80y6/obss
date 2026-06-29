using Mapster;
using MediatR;
using Obss.ServiceInventory.Application.Abstractions;
using Obss.ServiceInventory.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ServiceInventory.Application.Queries.GetDiscoveryJobs;

public sealed class GetDiscoveryJobsQueryHandler : IRequestHandler<GetDiscoveryJobsQuery, Result<List<DiscoveryJobDto>>>
{
    private readonly IResourceDiscoveryJobRepository _discoveryJobRepository;

    public GetDiscoveryJobsQueryHandler(IResourceDiscoveryJobRepository discoveryJobRepository)
    {
        _discoveryJobRepository = discoveryJobRepository;
    }

    public async Task<Result<List<DiscoveryJobDto>>> Handle(GetDiscoveryJobsQuery request, CancellationToken cancellationToken)
    {
        var jobs = await _discoveryJobRepository.GetByTenantAsync(request.TenantId, cancellationToken);
        return Result.Success(jobs.Adapt<List<DiscoveryJobDto>>());
    }
}
