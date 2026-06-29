using Mapster;
using MediatR;
using Obss.NetworkInventory.Domain.Entities;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.NetworkInventory.Application.Queries.GetTopologyMaps;

public sealed class GetTopologyMapsQueryHandler : IRequestHandler<GetTopologyMapsQuery, Result<IReadOnlyList<TopologyMapDto>>>
{
    private readonly IRepository<TopologyMap> _repository;

    public GetTopologyMapsQueryHandler(IRepository<TopologyMap> repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<TopologyMapDto>>> Handle(GetTopologyMapsQuery request, CancellationToken cancellationToken)
    {
        var maps = await _repository.GetAllAsync(cancellationToken);
        var result = maps
            .OrderByDescending(m => m.CreatedAt)
            .ToList()
            .Adapt<List<TopologyMapDto>>();

        return Result.Success<IReadOnlyList<TopologyMapDto>>(result);
    }
}
