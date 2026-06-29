using MediatR;
using Obss.ServiceInventory.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ServiceInventory.Application.Queries.GetUpstreamServices;

public sealed class GetUpstreamServicesQueryHandler : IRequestHandler<GetUpstreamServicesQuery, Result<List<Guid>>>
{
    private readonly IServiceTopologyRepository _topologyRepository;

    public GetUpstreamServicesQueryHandler(IServiceTopologyRepository topologyRepository)
    {
        _topologyRepository = topologyRepository;
    }

    public async Task<Result<List<Guid>>> Handle(GetUpstreamServicesQuery request, CancellationToken cancellationToken)
    {
        var links = await _topologyRepository.GetUpstreamLinksAsync(request.ServiceId, cancellationToken);

        var serviceIds = links
            .Where(l => l.TargetServiceId == request.ServiceId)
            .Select(l => l.SourceServiceId)
            .Distinct()
            .ToList();

        return Result.Success(serviceIds);
    }
}
