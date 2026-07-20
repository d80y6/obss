using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.StaticIp;

public sealed record AllocateStaticIpCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    string IpVersion,
    int IpCount,
    string CustomerSite,
    bool ReverseDnsRequired) : IRequest<Result<StaticIpLifecycleResult>>;
