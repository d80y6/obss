using FluentValidation;

namespace Obss.Subscriptions.Application.Commands.RenewSubscription;

internal sealed class RenewSubscriptionCommandValidator : AbstractValidator<RenewSubscriptionCommand>
{
    public RenewSubscriptionCommandValidator()
    {
        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("Subscription ID is required.");
    }
}
