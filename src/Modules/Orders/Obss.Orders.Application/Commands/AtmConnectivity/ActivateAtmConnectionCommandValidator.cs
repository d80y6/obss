using FluentValidation;

namespace Obss.Orders.Application.Commands.AtmConnectivity;

public sealed class ActivateAtmConnectionCommandValidator : AbstractValidator<ActivateAtmConnectionCommand>
{
    public ActivateAtmConnectionCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.OrderItemId).NotEmpty();
        RuleFor(x => x.SubscriptionId).NotEmpty();
        RuleFor(x => x.TerminalId).NotEmpty().MaximumLength(100);
        RuleFor(x => x.TerminalLocation).NotEmpty().MaximumLength(500);
        RuleFor(x => x.TerminalType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ConnectivityType).NotEmpty().Must(c => c is "MPLS" or "VPLS" or "DIA");
        RuleFor(x => x.BandwidthMbps).InclusiveBetween(1, 10000);
        RuleFor(x => x.VlanId).InclusiveBetween(1, 4094);
        RuleFor(x => x.HostEndpoint).NotEmpty().MaximumLength(255);
    }
}
