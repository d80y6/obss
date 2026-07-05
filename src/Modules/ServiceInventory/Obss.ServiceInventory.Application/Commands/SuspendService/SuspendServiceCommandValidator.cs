using FluentValidation;

namespace Obss.ServiceInventory.Application.Commands.SuspendService;

internal sealed class SuspendServiceCommandValidator : AbstractValidator<SuspendServiceCommand>
{
    public SuspendServiceCommandValidator()
    {
        RuleFor(x => x.ServiceId)
            .NotEmpty().WithMessage("Service ID is required.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required.")
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters.");
    }
}
