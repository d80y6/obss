using MediatR;
using Obss.ServiceInventory.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ServiceInventory.Application.Commands.CompleteDiscoveryJob;

public sealed record CompleteDiscoveryJobCommand(
    Guid JobId,
    int ResourcesFound,
    int ResourcesMatched,
    string? ErrorMessage) : IRequest<Result<DiscoveryJobDto>>;
