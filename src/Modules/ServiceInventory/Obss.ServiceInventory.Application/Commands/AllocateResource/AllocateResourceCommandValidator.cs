using FluentValidation;

namespace Obss.ServiceInventory.Application.Commands.AllocateResource;

public sealed class AllocateResourceCommandValidator : AbstractValidator<AllocateResourceCommand>
{
    public AllocateResourceCommandValidator()
    {
        RuleFor(x => x.ServiceId)
            .NotEmpty().WithMessage("Service ID is required.");

        RuleFor(x => x.ResourceType)
            .NotEmpty().WithMessage("Resource type is required.")
            .MaximumLength(50).WithMessage("Resource type must not exceed 50 characters.");

        RuleFor(x => x.ResourceIdentifier)
            .NotEmpty().WithMessage("Resource identifier is required.")
            .MaximumLength(200).WithMessage("Resource identifier must not exceed 200 characters.");
    }
}
