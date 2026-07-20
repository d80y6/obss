using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.WebHosting;

public sealed record ChangeWebHostingPlanCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    string NewHostingPlan,
    string? Reason) : IRequest<Result<WebHostingLifecycleResult>>;
