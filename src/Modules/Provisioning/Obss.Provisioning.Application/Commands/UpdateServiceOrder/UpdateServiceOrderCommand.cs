using MediatR;
using Obss.Provisioning.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Provisioning.Application.Commands.UpdateServiceOrder;

public sealed record UpdateServiceOrderCommand(
    Guid Id,
    string? Description,
    string? Category,
    string? Priority,
    DateTime? RequestedStartDate,
    DateTime? RequestedCompletionDate) : IRequest<Result<ServiceOrderDto>>;
