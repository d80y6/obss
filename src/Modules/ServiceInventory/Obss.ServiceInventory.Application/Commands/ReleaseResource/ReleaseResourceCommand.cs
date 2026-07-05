using MediatR;
using Obss.ServiceInventory.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ServiceInventory.Application.Commands.ReleaseResource;

public sealed record ReleaseResourceCommand(
    Guid ServiceId,
    Guid ResourceId) : IRequest<Result<ServiceDto>>;
