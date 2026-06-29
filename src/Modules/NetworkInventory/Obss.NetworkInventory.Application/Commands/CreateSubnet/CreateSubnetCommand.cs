using MediatR;
using Obss.NetworkInventory.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NetworkInventory.Application.Commands.CreateSubnet;

public sealed record CreateSubnetCommand(
    string TenantId,
    string Network,
    string Name,
    string? Description,
    string? Gateway,
    int VLANId,
    string? Location) : IRequest<Result<SubnetDto>>;
