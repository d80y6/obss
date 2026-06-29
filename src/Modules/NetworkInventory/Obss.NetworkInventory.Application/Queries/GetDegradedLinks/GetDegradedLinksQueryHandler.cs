using Mapster;
using MediatR;
using Obss.NetworkInventory.Application.DTOs;
using Obss.NetworkInventory.Domain.Entities;
using Obss.NetworkInventory.Domain.ValueObjects;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.NetworkInventory.Application.Queries.GetDegradedLinks;

public sealed class GetDegradedLinksQueryHandler : IRequestHandler<GetDegradedLinksQuery, Result<IReadOnlyList<ConnectivityLinkDto>>>
{
    private readonly IRepository<ConnectivityLink> _repository;

    public GetDegradedLinksQueryHandler(IRepository<ConnectivityLink> repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<ConnectivityLinkDto>>> Handle(GetDegradedLinksQuery request, CancellationToken cancellationToken)
    {
        var allLinks = await _repository.GetAllAsync(cancellationToken);
        var degraded = allLinks
            .Where(l => l.Status == LinkStatus.Degraded || l.Status == LinkStatus.Down)
            .ToList()
            .Adapt<List<ConnectivityLinkDto>>();

        return Result.Success<IReadOnlyList<ConnectivityLinkDto>>(degraded);
    }
}
