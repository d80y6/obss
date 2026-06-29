using MediatR;
using Obss.NetworkInventory.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NetworkInventory.Application.Queries.GetAvailableIPs;

public sealed record GetAvailableIPsQuery(Guid SubnetId) : IRequest<Result<IReadOnlyList<NetworkIpAddressDto>>>;
