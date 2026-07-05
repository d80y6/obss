using FluentValidation;

namespace Obss.Subscriptions.Application.Commands.CancelSubscription;

internal sealed class CancelSubscriptionCommandValidator : AbstractValidator<CancelSubscriptionCommand>
{
    public CancelSubscriptionCommandValidator()
    {
        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("Subscription ID is required.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required.")
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters.");

        RuleFor(x => x.EffectiveDate)
            .NotEmpty().WithMessage("Effective date is required.");
    }
}
