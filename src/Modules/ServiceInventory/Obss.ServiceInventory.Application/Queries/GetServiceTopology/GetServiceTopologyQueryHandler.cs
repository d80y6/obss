using Mapster;
using MediatR;
using Obss.ServiceInventory.Application.Abstractions;
using Obss.ServiceInventory.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ServiceInventory.Application.Queries.GetServiceTopology;

public sealed class GetServiceTopologyQueryHandler : IRequestHandler<GetServiceTopologyQuery, Result<ServiceTopologyDto>>
{
    private readonly IServiceTopologyRepository _topologyRepository;

    public GetServiceTopologyQueryHandler(IServiceTopologyRepository topologyRepository)
    {
        _topologyRepository = topologyRepository;
    }

    public async Task<Result<ServiceTopologyDto>> Handle(GetServiceTopologyQuery request, CancellationToken cancellationToken)
    {
        var topology = await _topologyRepository.GetByServiceIdWithLinksAsync(request.ServiceId, cancellationToken);

        if (topology is null)
            return Result.Failure<ServiceTopologyDto>(Error.NotFound("ServiceTopology", request.ServiceId));

        return Result.Success(topology.Adapt<ServiceTopologyDto>());
    }
}
