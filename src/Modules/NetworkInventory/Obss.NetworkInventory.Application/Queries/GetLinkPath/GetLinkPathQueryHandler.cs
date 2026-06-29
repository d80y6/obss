using Mapster;
using MediatR;
using Obss.NetworkInventory.Application.DTOs;
using Obss.NetworkInventory.Domain.Entities;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.NetworkInventory.Application.Queries.GetLinkPath;

public sealed class GetLinkPathQueryHandler : IRequestHandler<GetLinkPathQuery, Result<IReadOnlyList<ConnectivityLinkDto>>>
{
    private readonly IRepository<ConnectivityLink> _repository;

    public GetLinkPathQueryHandler(IRepository<ConnectivityLink> repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<ConnectivityLinkDto>>> Handle(GetLinkPathQuery request, CancellationToken cancellationToken)
    {
        var allLinks = await _repository.GetAllAsync(cancellationToken);

        var path = FindPath(allLinks.ToList(), request.SourceElementId, request.TargetElementId, []);

        return Result.Success<IReadOnlyList<ConnectivityLinkDto>>(path.Adapt<List<ConnectivityLinkDto>>());
    }

    private static List<ConnectivityLink> FindPath(
        List<ConnectivityLink> links,
        Guid currentId,
        Guid targetId,
        HashSet<Guid> visited)
    {
        if (currentId == targetId)
            return [];

        visited.Add(currentId);

        var outgoing = links.Where(l => l.SourceElementId == currentId && !visited.Contains(l.TargetElementId)).ToList();

        foreach (var link in outgoing)
        {
            if (link.TargetElementId == targetId)
                return [link];

            var subPath = FindPath(links, link.TargetElementId, targetId, visited);
            if (subPath.Count > 0)
            {
                subPath.Insert(0, link);
                return subPath;
            }
        }

        var incoming = links.Where(l => l.TargetElementId == currentId && !visited.Contains(l.SourceElementId)).ToList();

        foreach (var link in incoming)
        {
            if (link.SourceElementId == targetId)
                return [link];

            var subPath = FindPath(links, link.SourceElementId, targetId, visited);
            if (subPath.Count > 0)
            {
                subPath.Insert(0, link);
                return subPath;
            }
        }

        return [];
    }
}
