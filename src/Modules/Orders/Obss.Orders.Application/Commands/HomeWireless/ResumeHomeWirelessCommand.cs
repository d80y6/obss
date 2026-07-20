using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.HomeWireless;

public sealed record ResumeHomeWirelessCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    string? Reason) : IRequest<Result<HomeWirelessLifecycleResult>>;
