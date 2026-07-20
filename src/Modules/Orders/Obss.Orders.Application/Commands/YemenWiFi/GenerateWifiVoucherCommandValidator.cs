using FluentValidation;

namespace Obss.Orders.Application.Commands.YemenWiFi;

public sealed class GenerateWifiVoucherCommandValidator : AbstractValidator<GenerateWifiVoucherCommand>
{
    public GenerateWifiVoucherCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.OrderItemId).NotEmpty();
        RuleFor(x => x.SubscriptionId).NotEmpty();
        RuleFor(x => x.AccessPointId).NotEmpty().MaximumLength(100);
        RuleFor(x => x.VoucherCount).InclusiveBetween(1, 1000);
        RuleFor(x => x.ValidityHours).InclusiveBetween(1, 8760);
        RuleFor(x => x.BandwidthMbps).InclusiveBetween(1, 1000);
        RuleFor(x => x.DataAllowanceMb).InclusiveBetween(1, 1000000);
    }
}
