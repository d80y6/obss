using FluentValidation;

namespace Obss.Orders.Application.Commands.YemenWiFi;

public sealed class TerminateWifiAccessCommandValidator : AbstractValidator<TerminateWifiAccessCommand>
{
    public TerminateWifiAccessCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.OrderItemId).NotEmpty();
        RuleFor(x => x.SubscriptionId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
