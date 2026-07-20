using FluentValidation;

namespace Obss.Orders.Application.Commands.Colocation;

public sealed class ProvisionColocationCommandValidator : AbstractValidator<ProvisionColocationCommand>
{
    public ProvisionColocationCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.OrderItemId).NotEmpty();
        RuleFor(x => x.SubscriptionId).NotEmpty();
        RuleFor(x => x.RackUnits).InclusiveBetween(1, 100);
        RuleFor(x => x.PowerWatts).InclusiveBetween(100, 100000);
        RuleFor(x => x.BandwidthMbps).InclusiveBetween(1, 100000);
        RuleFor(x => x.CrossConnects).InclusiveBetween(0, 500);
    }
}
