using FluentValidation;

namespace Obss.Rating.Application.Commands.CreateRatingRule;

public sealed class CreateRatingRuleCommandValidator : AbstractValidator<CreateRatingRuleCommand>
{
    public CreateRatingRuleCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.")
            .When(x => x.Description is not null);

        RuleFor(x => x.RuleType)
            .NotEmpty().WithMessage("Rule type is required.")
            .Must(v => v is "Usage" or "Time" or "Volume" or "Flat")
            .WithMessage("Rule type must be Usage, Time, Volume, or Flat.");

        RuleFor(x => x.Priority)
            .GreaterThanOrEqualTo(0).WithMessage("Priority must be non-negative.");

        RuleFor(x => x.Tiers)
            .NotNull().WithMessage("At least one tier is required.")
            .Must(t => t.Count > 0).WithMessage("At least one tier is required.");

        RuleForEach(x => x.Tiers).ChildRules(tier =>
        {
            tier.RuleFor(t => t.FromUnit)
                .GreaterThanOrEqualTo(0).WithMessage("FromUnit must be non-negative.");

            tier.RuleFor(t => t.ToUnit)
                .GreaterThan(t => t.FromUnit).WithMessage("ToUnit must be greater than FromUnit.")
                .When(t => t.ToUnit.HasValue);

            tier.RuleFor(t => t.Rate)
                .GreaterThanOrEqualTo(0).WithMessage("Rate must be non-negative.");
        });
    }
}
