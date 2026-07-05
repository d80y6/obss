using FluentValidation;

namespace Obss.Subscriptions.Application.Commands.OverrideEntitlementLimit;

internal sealed class OverrideEntitlementLimitCommandValidator : AbstractValidator<OverrideEntitlementLimitCommand>
{
    public OverrideEntitlementLimitCommandValidator()
    {
        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("Subscription ID is required.");

        RuleFor(x => x.EntitlementType)
            .NotEmpty().WithMessage("Entitlement type is required.")
            .MaximumLength(100).WithMessage("Entitlement type must not exceed 100 characters.");

        RuleFor(x => x.NewLimit)
            .GreaterThanOrEqualTo(0).WithMessage("New limit must be zero or greater.");
    }
}
