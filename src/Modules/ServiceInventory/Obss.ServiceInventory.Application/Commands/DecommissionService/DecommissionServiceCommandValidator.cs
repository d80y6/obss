using FluentValidation;

namespace Obss.ServiceInventory.Application.Commands.DecommissionService;

internal sealed class DecommissionServiceCommandValidator : AbstractValidator<DecommissionServiceCommand>
{
    public DecommissionServiceCommandValidator()
    {
        RuleFor(x => x.ServiceId)
            .NotEmpty().WithMessage("Service ID is required.");
    }
}
