using MediatR;
using Obss.ServiceInventory.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ServiceInventory.Application.Commands.SuspendService;

public sealed record SuspendServiceCommand(Guid ServiceId, string Reason) : IRequest<Result<ServiceDto>>;
