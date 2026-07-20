using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.WebHosting;

public sealed record ProvisionWebHostingCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    string DomainName,
    string HostingPlan,
    int DiskSpaceMb,
    int BandwidthGb,
    int EmailAccounts,
    int DatabaseCount,
    bool SslCertificate,
    string? CmsType) : IRequest<Result<WebHostingLifecycleResult>>;

public sealed record WebHostingLifecycleResult(
    Guid CorrelationId,
    string Status,
    string Message,
    string MessageAr,
    Guid? ProvisioningJobId);
