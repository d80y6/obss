using MediatR;
using Obss.ServiceInventory.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ServiceInventory.Application.Commands.UpdateService;

public sealed record UpdateServiceCommand(
    Guid ServiceId,
    string? Configuration,
    string? Location) : IRequest<Result<ServiceDto>>;
