using FluentValidation;

namespace Obss.Collections.Application.Commands.CreateDunningPolicy;

public sealed class CreateDunningPolicyCommandValidator : AbstractValidator<CreateDunningPolicyCommand>
{
    public CreateDunningPolicyCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Policy name is required.")
            .MaximumLength(200).WithMessage("Policy name must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");

        RuleFor(x => x.MaxDunningLevel)
            .GreaterThan(0).WithMessage("Max dunning level must be greater than zero.")
            .LessThanOrEqualTo(10).WithMessage("Max dunning level must not exceed 10.");

        RuleFor(x => x.DaysBetweenActions)
            .GreaterThan(0).WithMessage("Days between actions must be greater than zero.");

        RuleFor(x => x.EscalationAfterDays)
            .GreaterThan(0).WithMessage("Escalation after days must be greater than zero.");
    }
}
