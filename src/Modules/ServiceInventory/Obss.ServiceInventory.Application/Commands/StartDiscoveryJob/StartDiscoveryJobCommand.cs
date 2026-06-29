using MediatR;
using Obss.ServiceInventory.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ServiceInventory.Application.Commands.StartDiscoveryJob;

public sealed record StartDiscoveryJobCommand(
    Guid TenantId,
    string DiscoveryType,
    string? Configuration,
    string CreatedBy) : IRequest<Result<DiscoveryJobDto>>;
