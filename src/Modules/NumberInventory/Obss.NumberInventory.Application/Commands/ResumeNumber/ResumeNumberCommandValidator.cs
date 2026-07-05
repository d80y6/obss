using FluentValidation;

namespace Obss.NumberInventory.Application.Commands.ResumeNumber;

internal sealed class ResumeNumberCommandValidator : AbstractValidator<ResumeNumberCommand>
{
    public ResumeNumberCommandValidator()
    {
        RuleFor(x => x.NumberId)
            .NotEmpty().WithMessage("Number ID is required.");
    }
}
