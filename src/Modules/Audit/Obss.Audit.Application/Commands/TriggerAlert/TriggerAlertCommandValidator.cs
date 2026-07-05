using FluentValidation;

namespace Obss.Audit.Application.Commands.TriggerAlert;

internal sealed class TriggerAlertCommandValidator : AbstractValidator<TriggerAlertCommand>
{
    public TriggerAlertCommandValidator()
    {
        RuleFor(x => x.Severity)
            .NotEmpty().WithMessage("Severity is required.")
            .MaximumLength(50).WithMessage("Severity must not exceed 50 characters.");

        RuleFor(x => x.AlertType)
            .NotEmpty().WithMessage("Alert type is required.")
            .MaximumLength(100).WithMessage("Alert type must not exceed 100 characters.");

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required.")
            .MaximumLength(1000).WithMessage("Message must not exceed 1000 characters.");

        RuleFor(x => x.EntityType)
            .MaximumLength(200).WithMessage("Entity type must not exceed 200 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.EntityType));

        RuleFor(x => x.EntityId)
            .MaximumLength(200).WithMessage("Entity ID must not exceed 200 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.EntityId));
    }
}
