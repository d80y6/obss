using MediatR;
using Obss.NetworkInventory.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NetworkInventory.Application.Commands.AddInterface;

public sealed record AddInterfaceCommand(
    Guid NetworkElementId,
    string Name,
    string? Description,
    string InterfaceType,
    int Speed,
    string? MacAddress,
    int MTU,
    Guid? ConnectedToInterfaceId) : IRequest<Result<NetworkInterfaceDto>>;
