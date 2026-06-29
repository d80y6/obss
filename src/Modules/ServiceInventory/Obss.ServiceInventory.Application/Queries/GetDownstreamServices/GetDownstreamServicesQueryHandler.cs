using MediatR;
using Obss.ServiceInventory.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ServiceInventory.Application.Queries.GetDownstreamServices;

public sealed class GetDownstreamServicesQueryHandler : IRequestHandler<GetDownstreamServicesQuery, Result<List<Guid>>>
{
    private readonly IServiceTopologyRepository _topologyRepository;

    public GetDownstreamServicesQueryHandler(IServiceTopologyRepository topologyRepository)
    {
        _topologyRepository = topologyRepository;
    }

    public async Task<Result<List<Guid>>> Handle(GetDownstreamServicesQuery request, CancellationToken cancellationToken)
    {
        var links = await _topologyRepository.GetDownstreamLinksAsync(request.ServiceId, cancellationToken);

        var serviceIds = links
            .Where(l => l.SourceServiceId == request.ServiceId)
            .Select(l => l.TargetServiceId)
            .Distinct()
            .ToList();

        return Result.Success(serviceIds);
    }
}
