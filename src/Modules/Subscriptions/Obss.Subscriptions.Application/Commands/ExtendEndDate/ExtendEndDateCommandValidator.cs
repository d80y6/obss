using FluentValidation;

namespace Obss.Subscriptions.Application.Commands.ExtendEndDate;

internal sealed class ExtendEndDateCommandValidator : AbstractValidator<ExtendEndDateCommand>
{
    public ExtendEndDateCommandValidator()
    {
        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("Subscription ID is required.");

        RuleFor(x => x.NewEndDate)
            .NotEmpty().WithMessage("New end date is required.")
            .GreaterThan(DateTime.UtcNow).WithMessage("New end date must be in the future.");
    }
}
