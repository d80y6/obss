using MediatR;
using Obss.NetworkInventory.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NetworkInventory.Application.Commands.AllocateIP;

public sealed record AllocateIPCommand(
    Guid NetworkElementId,
    Guid? NetworkInterfaceId,
    string IPAddress,
    string SubnetMask,
    string? Gateway,
    string AddressType,
    string? AssignedTo) : IRequest<Result<NetworkIpAddressDto>>;
