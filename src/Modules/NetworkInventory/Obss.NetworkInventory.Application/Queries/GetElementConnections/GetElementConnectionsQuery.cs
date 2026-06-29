using MediatR;
using Obss.NetworkInventory.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NetworkInventory.Application.Queries.GetElementConnections;

public sealed record GetElementConnectionsQuery(Guid ElementId) : IRequest<Result<IReadOnlyList<ConnectivityLinkDto>>>;
