using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.Lte;

public sealed record Change4GPlanCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    string ApnName,
    string QosProfile,
    int DataAllowanceMb,
    int ValidityDays,
    string? Reason) : IRequest<Result<LifecycleResult>>;
