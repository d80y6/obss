using FluentValidation;

namespace Obss.ServiceInventory.Application.Commands.ActivateService;

internal sealed class ActivateServiceCommandValidator : AbstractValidator<ActivateServiceCommand>
{
    public ActivateServiceCommandValidator()
    {
        RuleFor(x => x.ServiceId)
            .NotEmpty().WithMessage("Service ID is required.");
    }
}
