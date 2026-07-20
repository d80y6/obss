using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.StaticIp;

public sealed record ChangeStaticIpCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    int? NewIpCount,
    string? NewCustomerSite,
    bool? ReverseDnsRequired,
    string? Reason) : IRequest<Result<StaticIpLifecycleResult>>;
