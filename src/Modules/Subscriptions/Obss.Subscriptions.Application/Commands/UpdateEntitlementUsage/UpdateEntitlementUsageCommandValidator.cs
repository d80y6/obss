using FluentValidation;

namespace Obss.Subscriptions.Application.Commands.UpdateEntitlementUsage;

internal sealed class UpdateEntitlementUsageCommandValidator : AbstractValidator<UpdateEntitlementUsageCommand>
{
    public UpdateEntitlementUsageCommandValidator()
    {
        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("Subscription ID is required.");

        RuleFor(x => x.EntitlementType)
            .NotEmpty().WithMessage("Entitlement type is required.")
            .MaximumLength(100).WithMessage("Entitlement type must not exceed 100 characters.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero.");
    }
}
