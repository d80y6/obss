using FluentValidation;

namespace Obss.Audit.Application.Commands.AcknowledgeAlert;

internal sealed class AcknowledgeAlertCommandValidator : AbstractValidator<AcknowledgeAlertCommand>
{
    public AcknowledgeAlertCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Alert ID is required.");
    }
}
