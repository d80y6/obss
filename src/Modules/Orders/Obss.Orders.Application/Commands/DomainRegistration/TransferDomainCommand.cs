using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.DomainRegistration;

public sealed record TransferDomainCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    string AuthCode,
    string? Reason) : IRequest<Result<DomainLifecycleResult>>;
