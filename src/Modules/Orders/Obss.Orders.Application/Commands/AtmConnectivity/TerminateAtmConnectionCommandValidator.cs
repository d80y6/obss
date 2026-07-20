using FluentValidation;

namespace Obss.Orders.Application.Commands.AtmConnectivity;

public sealed class TerminateAtmConnectionCommandValidator : AbstractValidator<TerminateAtmConnectionCommand>
{
    public TerminateAtmConnectionCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.OrderItemId).NotEmpty();
        RuleFor(x => x.SubscriptionId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
