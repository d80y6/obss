using FluentValidation;

namespace Obss.ServiceInventory.Application.Commands.ResumeService;

internal sealed class ResumeServiceCommandValidator : AbstractValidator<ResumeServiceCommand>
{
    public ResumeServiceCommandValidator()
    {
        RuleFor(x => x.ServiceId)
            .NotEmpty().WithMessage("Service ID is required.");
    }
}
