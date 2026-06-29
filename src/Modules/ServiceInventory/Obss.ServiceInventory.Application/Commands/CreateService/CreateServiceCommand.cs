using MediatR;
using Obss.ServiceInventory.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ServiceInventory.Application.Commands.CreateService;

public sealed record CreateServiceCommand(
    Guid CustomerId,
    Guid SubscriptionId,
    string ServiceType,
    string ServiceIdentifier,
    string? Location,
    string? Configuration) : IRequest<Result<ServiceDto>>;
