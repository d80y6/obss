using FluentValidation;

namespace Obss.Subscriptions.Application.Commands.SetSubscriptionEntitlements;

internal sealed class SetSubscriptionEntitlementsCommandValidator : AbstractValidator<SetSubscriptionEntitlementsCommand>
{
    public SetSubscriptionEntitlementsCommandValidator()
    {
        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("Subscription ID is required.");

        RuleFor(x => x.Entitlements)
            .NotNull().WithMessage("Entitlements list is required.")
            .NotEmpty().WithMessage("At least one entitlement is required.");
    }
}
