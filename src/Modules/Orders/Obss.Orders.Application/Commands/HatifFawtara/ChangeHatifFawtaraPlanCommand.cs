using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.HatifFawtara;

public sealed record ChangeHatifFawtaraPlanCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    string TelephoneNumber,
    string BillingCycle,
    decimal CreditLimit,
    string ServicePackage,
    string? Reason) : IRequest<Result<HatifFawtaraLifecycleResult>>;
