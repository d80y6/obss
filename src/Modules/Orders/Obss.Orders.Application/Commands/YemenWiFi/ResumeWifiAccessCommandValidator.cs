using FluentValidation;

namespace Obss.Orders.Application.Commands.YemenWiFi;

public sealed class ResumeWifiAccessCommandValidator : AbstractValidator<ResumeWifiAccessCommand>
{
    public ResumeWifiAccessCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.OrderItemId).NotEmpty();
        RuleFor(x => x.SubscriptionId).NotEmpty();
    }
}
