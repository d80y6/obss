using MediatR;
using Obss.Provisioning.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Provisioning.Application.Commands.CreateServiceOrder;

public sealed record CreateServiceOrderCommand(
    Guid TenantId,
    string? ExternalId,
    string? Description,
    string? Category,
    string? Priority,
    List<CreateServiceOrderItemRequest> Items) : IRequest<Result<ServiceOrderDto>>;

public sealed record CreateServiceOrderItemRequest(
    Guid? ServiceId,
    string Action,
    int Quantity,
    string? Description,
    DateTime? RequestedStartDate,
    DateTime? RequestedCompletionDate);
