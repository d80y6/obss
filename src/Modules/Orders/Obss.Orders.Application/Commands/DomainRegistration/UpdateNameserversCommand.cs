using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.DomainRegistration;

public sealed record UpdateNameserversCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    string[] Nameservers) : IRequest<Result<DomainLifecycleResult>>;
