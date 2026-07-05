using MediatR;
using Obss.NetworkInventory.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NetworkInventory.Application.Commands.UpdateSubnet;

public sealed record UpdateSubnetCommand(
    Guid Id,
    string Name,
    string? Description,
    string? Gateway,
    int VLANId,
    string? Location) : IRequest<Result<SubnetDto>>;
