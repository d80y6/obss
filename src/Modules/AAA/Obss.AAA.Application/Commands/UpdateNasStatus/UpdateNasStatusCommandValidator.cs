using FluentValidation;

namespace Obss.AAA.Application.Commands.UpdateNasStatus;

public sealed class UpdateNasStatusCommandValidator : AbstractValidator<UpdateNasStatusCommand>
{
    public UpdateNasStatusCommandValidator()
    {
        RuleFor(x => x.NasId)
            .NotEmpty().WithMessage("NAS ID is required.");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required.")
            .Must(s => s.Equals("Active", StringComparison.OrdinalIgnoreCase) ||
                       s.Equals("Inactive", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Status must be 'Active' or 'Inactive'.");
    }
}
