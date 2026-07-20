using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.DomainRegistration;

public sealed record RegisterDomainCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    string DomainName,
    string Tld,
    int RegistrationPeriodYears,
    string[] Nameservers,
    string RegistrantName,
    string RegistrantEmail,
    bool AutoRenew) : IRequest<Result<DomainLifecycleResult>>;

public sealed record DomainLifecycleResult(
    Guid CorrelationId,
    string Status,
    string Message,
    string MessageAr,
    Guid? ProvisioningJobId);
