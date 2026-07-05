using FluentValidation;

namespace Obss.Subscriptions.Application.Commands.SuspendSubscription;

internal sealed class SuspendSubscriptionCommandValidator : AbstractValidator<SuspendSubscriptionCommand>
{
    public SuspendSubscriptionCommandValidator()
    {
        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("Subscription ID is required.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required.")
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters.");
    }
}
