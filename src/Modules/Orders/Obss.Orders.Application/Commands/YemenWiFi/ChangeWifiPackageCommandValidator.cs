using FluentValidation;

namespace Obss.Orders.Application.Commands.YemenWiFi;

public sealed class ChangeWifiPackageCommandValidator : AbstractValidator<ChangeWifiPackageCommand>
{
    public ChangeWifiPackageCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.OrderItemId).NotEmpty();
        RuleFor(x => x.SubscriptionId).NotEmpty();
        RuleFor(x => x.PackageType).NotEmpty().Must(x => x is "daily" or "weekly" or "monthly");
        RuleFor(x => x.ValidityDays).InclusiveBetween(1, 365);
        RuleFor(x => x.BandwidthMbps).InclusiveBetween(1, 1000);
    }
}
