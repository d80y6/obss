using MediatR;
using Obss.ServiceInventory.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ServiceInventory.Application.Commands.ActivateService;

public sealed record ActivateServiceCommand(Guid ServiceId) : IRequest<Result<ServiceDto>>;
