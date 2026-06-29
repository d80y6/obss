using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NetworkInventory.Application.Commands.SaveTopologyMap;

public sealed record SaveTopologyMapCommand(
    string Name,
    string? Description,
    string? Configuration) : IRequest<Result<Guid>>;
