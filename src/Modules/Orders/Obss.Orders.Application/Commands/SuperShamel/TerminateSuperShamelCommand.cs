using MediatR;
using Obss.Orders.Application.Commands.Common;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.SuperShamel;

public sealed record TerminateSuperShamelCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    string Reason,
    DateTime? RequestedTerminationDate) : IRequest<Result<LifecycleResult>>;
