using FluentValidation;

namespace Obss.Orders.Application.Commands.Vps;

public sealed class TerminateVpsCommandValidator : AbstractValidator<TerminateVpsCommand>
{
    public TerminateVpsCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.OrderItemId).NotEmpty();
        RuleFor(x => x.SubscriptionId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
