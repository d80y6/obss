using FluentValidation;

namespace Obss.Orders.Application.Commands.YemenWiFi;

public sealed class SuspendWifiAccessCommandValidator : AbstractValidator<SuspendWifiAccessCommand>
{
    public SuspendWifiAccessCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.OrderItemId).NotEmpty();
        RuleFor(x => x.SubscriptionId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
