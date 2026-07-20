using FluentValidation;

namespace Obss.Orders.Application.Commands.Colocation;

public sealed class ChangeColocationCommandValidator : AbstractValidator<ChangeColocationCommand>
{
    public ChangeColocationCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.OrderItemId).NotEmpty();
        RuleFor(x => x.SubscriptionId).NotEmpty();
        RuleFor(x => x.NewRackUnits).InclusiveBetween(1, 100).When(x => x.NewRackUnits.HasValue);
        RuleFor(x => x.NewPowerWatts).InclusiveBetween(100, 100000).When(x => x.NewPowerWatts.HasValue);
        RuleFor(x => x.NewBandwidthMbps).InclusiveBetween(1, 100000).When(x => x.NewBandwidthMbps.HasValue);
        RuleFor(x => x.NewCrossConnects).InclusiveBetween(0, 500).When(x => x.NewCrossConnects.HasValue);
    }
}
