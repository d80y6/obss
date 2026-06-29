using MediatR;
using Obss.NetworkInventory.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NetworkInventory.Application.Queries.GetNetworkTopology;

public sealed record GetNetworkTopologyQuery() : IRequest<Result<NetworkTopologyDto>>;
