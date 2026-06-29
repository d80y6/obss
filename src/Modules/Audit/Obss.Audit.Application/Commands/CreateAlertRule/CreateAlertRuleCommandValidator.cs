using FluentValidation;

namespace Obss.Audit.Application.Commands.CreateAlertRule;

public sealed class CreateAlertRuleCommandValidator : AbstractValidator<CreateAlertRuleCommand>
{
    public CreateAlertRuleCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.AlertType)
            .NotEmpty().WithMessage("Alert type is required.");

        RuleFor(x => x.Severity)
            .NotEmpty().WithMessage("Severity is required.");

        RuleFor(x => x.Threshold)
            .GreaterThan(0).WithMessage("Threshold must be greater than 0.");

        RuleFor(x => x.WindowMinutes)
            .GreaterThan(0).WithMessage("Window minutes must be greater than 0.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}
