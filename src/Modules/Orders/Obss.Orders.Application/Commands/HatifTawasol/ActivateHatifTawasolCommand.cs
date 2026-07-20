using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.HatifTawasol;

public sealed record ActivateHatifTawasolCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    string TelephoneNumber,
    string CustomerName,
    string CustomerNameAr,
    string ServicePackage,
    bool IncludesCallForwarding,
    bool IncludesCallWaiting,
    bool IncludesCallerId,
    bool IncludesVoicemail,
    bool IncludesCallBarring) : IRequest<Result<HatifTawasolLifecycleResult>>;

public sealed record HatifTawasolLifecycleResult(
    Guid CorrelationId,
    string Status,
    string Message,
    string MessageAr,
    Guid? ProvisioningJobId);
