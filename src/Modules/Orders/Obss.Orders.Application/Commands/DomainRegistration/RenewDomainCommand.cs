using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.DomainRegistration;

public sealed record RenewDomainCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    int RenewalPeriodYears,
    string? Reason) : IRequest<Result<DomainLifecycleResult>>;
