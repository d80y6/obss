using FluentValidation;

namespace Obss.Orders.Application.Commands.YemenWiFi;

public sealed class ActivateWifiAccessCommandValidator : AbstractValidator<ActivateWifiAccessCommand>
{
    public ActivateWifiAccessCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.OrderItemId).NotEmpty();
        RuleFor(x => x.SubscriptionId).NotEmpty();
        RuleFor(x => x.SiteName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.SiteNameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.AccessPointId).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PackageType).NotEmpty().Must(x => x is "daily" or "weekly" or "monthly");
        RuleFor(x => x.ValidityDays).InclusiveBetween(1, 365);
        RuleFor(x => x.BandwidthMbps).InclusiveBetween(1, 1000);
        RuleFor(x => x.Username).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Password).NotEmpty().MaximumLength(100);
    }
}
