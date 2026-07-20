using FluentValidation;

namespace Obss.Orders.Application.Commands.AtmConnectivity;

public sealed class ChangeAtmConnectionCommandValidator : AbstractValidator<ChangeAtmConnectionCommand>
{
    public ChangeAtmConnectionCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.OrderItemId).NotEmpty();
        RuleFor(x => x.SubscriptionId).NotEmpty();
        RuleFor(x => x.NewBandwidthMbps).InclusiveBetween(1, 10000).When(x => x.NewBandwidthMbps.HasValue);
        RuleFor(x => x.NewVlanId).InclusiveBetween(1, 4094).When(x => x.NewVlanId.HasValue);
        RuleFor(x => x.NewHostEndpoint).MaximumLength(255).When(x => x.NewHostEndpoint is not null);
    }
}
