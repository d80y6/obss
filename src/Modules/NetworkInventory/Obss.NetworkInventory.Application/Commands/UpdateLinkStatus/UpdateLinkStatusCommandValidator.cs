using FluentValidation;

namespace Obss.NetworkInventory.Application.Commands.UpdateLinkStatus;

internal sealed class UpdateLinkStatusCommandValidator : AbstractValidator<UpdateLinkStatusCommand>
{
    public UpdateLinkStatusCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Link ID is required.");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required.")
            .MaximumLength(50).WithMessage("Status must not exceed 50 characters.");

        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Reason));
    }
}
