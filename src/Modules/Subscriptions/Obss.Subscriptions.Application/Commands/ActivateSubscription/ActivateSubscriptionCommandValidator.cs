using FluentValidation;

namespace Obss.Subscriptions.Application.Commands.ActivateSubscription;

internal sealed class ActivateSubscriptionCommandValidator : AbstractValidator<ActivateSubscriptionCommand>
{
    public ActivateSubscriptionCommandValidator()
    {
        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("Subscription ID is required.");
    }
}
