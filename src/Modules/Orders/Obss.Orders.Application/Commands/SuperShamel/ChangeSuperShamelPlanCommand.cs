using MediatR;
using Obss.Orders.Application.Commands.Common;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.SuperShamel;

public sealed record ChangeSuperShamelPlanCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    int? NewFtthSpeedMbps,
    string? NewHatifTawasolPackage,
    string? NewYemen4GDataPlan,
    string? Reason) : IRequest<Result<LifecycleResult>>;
