using FluentValidation;

namespace Obss.Orders.Application.Commands.AtmConnectivity;

public sealed class ResumeAtmConnectionCommandValidator : AbstractValidator<ResumeAtmConnectionCommand>
{
    public ResumeAtmConnectionCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.OrderItemId).NotEmpty();
        RuleFor(x => x.SubscriptionId).NotEmpty();
    }
}
