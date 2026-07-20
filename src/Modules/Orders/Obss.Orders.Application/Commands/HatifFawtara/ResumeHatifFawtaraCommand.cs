using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.HatifFawtara;

public sealed record ResumeHatifFawtaraCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    string? Reason) : IRequest<Result<HatifFawtaraLifecycleResult>>;
