using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.Adsl;

public sealed record ResumeAdslCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    string? Reason) : IRequest<Result<AdslLifecycleResult>>;
