using FluentValidation;

namespace Obss.Orders.Application.Commands.Colocation;

public sealed class TerminateColocationCommandValidator : AbstractValidator<TerminateColocationCommand>
{
    public TerminateColocationCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.OrderItemId).NotEmpty();
        RuleFor(x => x.SubscriptionId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
