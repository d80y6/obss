using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.SupplementaryTelephone;

public sealed record DeactivateSupplementaryServiceCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    string TelephoneNumber,
    ServiceFeature ServiceFeature,
    string Reason) : IRequest<Result<SupplementaryServiceLifecycleResult>>;
