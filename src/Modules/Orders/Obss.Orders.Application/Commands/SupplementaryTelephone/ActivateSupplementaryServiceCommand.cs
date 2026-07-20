using System.Text.Json;
using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.SupplementaryTelephone;

public enum ServiceFeature
{
    CallForwarding,
    CallWaiting,
    CallerId,
    Voicemail,
    CallBarring,
    ThreeWayCalling,
    ConferenceCall
}

public sealed record ActivateSupplementaryServiceCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    string TelephoneNumber,
    ServiceFeature ServiceFeature,
    JsonDocument Configuration) : IRequest<Result<SupplementaryServiceLifecycleResult>>;

public sealed record SupplementaryServiceLifecycleResult(
    Guid CorrelationId,
    string Status,
    string Message,
    string MessageAr,
    Guid? ProvisioningJobId);
