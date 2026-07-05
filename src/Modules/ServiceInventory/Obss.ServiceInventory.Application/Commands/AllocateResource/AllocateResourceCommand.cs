using MediatR;
using Obss.ServiceInventory.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ServiceInventory.Application.Commands.AllocateResource;

public sealed record AllocateResourceCommand(
    Guid ServiceId,
    string ResourceType,
    string ResourceIdentifier) : IRequest<Result<ServiceDto>>;
