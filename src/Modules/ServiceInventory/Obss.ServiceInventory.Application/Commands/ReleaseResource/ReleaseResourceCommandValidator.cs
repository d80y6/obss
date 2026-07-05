using FluentValidation;

namespace Obss.ServiceInventory.Application.Commands.ReleaseResource;

public sealed class ReleaseResourceCommandValidator : AbstractValidator<ReleaseResourceCommand>
{
    public ReleaseResourceCommandValidator()
    {
        RuleFor(x => x.ServiceId)
            .NotEmpty().WithMessage("Service ID is required.");

        RuleFor(x => x.ResourceId)
            .NotEmpty().WithMessage("Resource ID is required.");
    }
}
