using Mapster;
using MediatR;
using Obss.NetworkInventory.Application.Abstractions;
using Obss.NetworkInventory.Application.DTOs;
using Obss.NetworkInventory.Domain.Entities;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NetworkInventory.Application.Queries.GetNetworkElements;

public sealed class GetNetworkElementsQueryHandler : IRequestHandler<GetNetworkElementsQuery, Result<IReadOnlyList<NetworkElementDto>>>
{
    private readonly INetworkElementRepository _repository;

    public GetNetworkElementsQueryHandler(INetworkElementRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<NetworkElementDto>>> Handle(GetNetworkElementsQuery request, CancellationToken cancellationToken)
    {
        var elements = await _repository.GetFilteredAsync(
            request.Type,
            request.Status,
            request.Location,
            request.Offset,
            request.Limit,
            cancellationToken);

        var result = elements.Adapt<List<NetworkElementDto>>();
        return Result.Success<IReadOnlyList<NetworkElementDto>>(result);
    }
}
