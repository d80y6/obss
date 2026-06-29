using Mapster;
using MediatR;
using Obss.NetworkInventory.Application.DTOs;
using Obss.NetworkInventory.Domain.Entities;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.NetworkInventory.Application.Queries.GetElementConnections;

public sealed class GetElementConnectionsQueryHandler : IRequestHandler<GetElementConnectionsQuery, Result<IReadOnlyList<ConnectivityLinkDto>>>
{
    private readonly IRepository<ConnectivityLink> _repository;

    public GetElementConnectionsQueryHandler(IRepository<ConnectivityLink> repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<ConnectivityLinkDto>>> Handle(GetElementConnectionsQuery request, CancellationToken cancellationToken)
    {
        var allLinks = await _repository.GetAllAsync(cancellationToken);
        var connections = allLinks
            .Where(l => l.SourceElementId == request.ElementId || l.TargetElementId == request.ElementId)
            .ToList()
            .Adapt<List<ConnectivityLinkDto>>();

        return Result.Success<IReadOnlyList<ConnectivityLinkDto>>(connections);
    }
}
