using MediatR;
using Obss.ServiceInventory.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ServiceInventory.Application.Commands.DecommissionService;

public sealed record DecommissionServiceCommand(Guid ServiceId) : IRequest<Result<ServiceDto>>;
