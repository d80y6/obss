using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.HatifFawtara;

public sealed record ActivateHatifFawtaraCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    string TelephoneNumber,
    string CustomerName,
    string CustomerNameAr,
    string BillingCycle,
    decimal CreditLimit,
    decimal DepositAmount,
    string ServicePackage) : IRequest<Result<HatifFawtaraLifecycleResult>>;

public sealed record HatifFawtaraLifecycleResult(
    Guid CorrelationId,
    string Status,
    string Message,
    string MessageAr,
    Guid? ProvisioningJobId);
