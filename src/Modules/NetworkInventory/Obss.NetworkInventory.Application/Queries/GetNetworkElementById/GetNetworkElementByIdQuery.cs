using MediatR;
using Obss.NetworkInventory.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NetworkInventory.Application.Queries.GetNetworkElementById;

public sealed record GetNetworkElementByIdQuery(Guid Id) : IRequest<Result<NetworkElementDto>>;
