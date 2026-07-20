using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.Dia;

public sealed record TerminateDiaCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    string Reason,
    DateTime? RequestedTerminationDate) : IRequest<Result<DiaLifecycleResult>>;
