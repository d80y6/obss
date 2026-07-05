using FluentValidation;

namespace Obss.Subscriptions.Application.Commands.ChangeQuantity;

internal sealed class ChangeQuantityCommandValidator : AbstractValidator<ChangeQuantityCommand>
{
    public ChangeQuantityCommandValidator()
    {
        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("Subscription ID is required.");

        RuleFor(x => x.NewQuantity)
            .GreaterThanOrEqualTo(1).WithMessage("Quantity must be at least 1.");
    }
}
