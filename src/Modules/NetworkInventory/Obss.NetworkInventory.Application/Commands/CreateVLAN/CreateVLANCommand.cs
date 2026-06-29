using MediatR;
using Obss.NetworkInventory.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NetworkInventory.Application.Commands.CreateVLAN;

public sealed record CreateVLANCommand(
    string TenantId,
    int VLANId,
    string Name,
    string? Description,
    string? Location) : IRequest<Result<VLANDto>>;
