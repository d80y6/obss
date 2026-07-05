using FluentValidation;

namespace Obss.ServiceInventory.Application.Commands.CompleteDiscoveryJob;

internal sealed class CompleteDiscoveryJobCommandValidator : AbstractValidator<CompleteDiscoveryJobCommand>
{
    public CompleteDiscoveryJobCommandValidator()
    {
        RuleFor(x => x.JobId)
            .NotEmpty().WithMessage("Job ID is required.");

        RuleFor(x => x.ResourcesFound)
            .GreaterThanOrEqualTo(0).WithMessage("Resources found must be zero or greater.");

        RuleFor(x => x.ResourcesMatched)
            .GreaterThanOrEqualTo(0).WithMessage("Resources matched must be zero or greater.");

        RuleFor(x => x.ErrorMessage)
            .MaximumLength(2000).WithMessage("Error message must not exceed 2000 characters.")
            .When(x => x.ErrorMessage is not null);
    }
}
