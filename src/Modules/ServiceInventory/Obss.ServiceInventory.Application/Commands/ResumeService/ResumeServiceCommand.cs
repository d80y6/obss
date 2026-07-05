using MediatR;
using Obss.ServiceInventory.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ServiceInventory.Application.Commands.ResumeService;

public sealed record ResumeServiceCommand(Guid ServiceId) : IRequest<Result<ServiceDto>>;
