using MediatR;
using Obss.NetworkInventory.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NetworkInventory.Application.Queries.GetNetworkElements;

public sealed record GetNetworkElementsQuery(
    string? Type,
    string? Status,
    string? Location,
    int Offset = 0,
    int Limit = 20) : IRequest<Result<IReadOnlyList<NetworkElementDto>>>;
