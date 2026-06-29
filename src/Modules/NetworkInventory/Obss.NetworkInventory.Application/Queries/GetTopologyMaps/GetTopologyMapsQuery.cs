using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NetworkInventory.Application.Queries.GetTopologyMaps;

public sealed record TopologyMapDto(
    Guid Id,
    string Name,
    string? Description,
    string? Configuration,
    DateTime CreatedAt);

public sealed record GetTopologyMapsQuery() : IRequest<Result<IReadOnlyList<TopologyMapDto>>>;
