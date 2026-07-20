using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.HatifTawasol;

public sealed record ChangeHatifTawasolFeaturesCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    string TelephoneNumber,
    bool IncludesCallForwarding,
    bool IncludesCallWaiting,
    bool IncludesCallerId,
    bool IncludesVoicemail,
    bool IncludesCallBarring,
    string? Reason) : IRequest<Result<HatifTawasolLifecycleResult>>;
