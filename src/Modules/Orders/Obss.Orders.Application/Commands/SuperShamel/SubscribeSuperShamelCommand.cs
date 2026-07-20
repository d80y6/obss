using MediatR;
using Obss.Orders.Application.Commands.Common;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.SuperShamel;

public sealed record SubscribeSuperShamelCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    int FtthSpeedMbps,
    string FtthOntSerial,
    string FtthLoid,
    string HatifTawasolTelephoneNumber,
    string HatifTawasolPackage,
    string Yemen4GIccid,
    string Yemen4GImsi,
    string Yemen4GMsisdn,
    string InstallationAddress,
    int ContractMonths) : IRequest<Result<LifecycleResult>>;
