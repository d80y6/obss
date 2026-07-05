using FluentValidation;

namespace Obss.Provisioning.Application.Commands.FailProvisioningJob;

internal sealed class FailProvisioningJobCommandValidator : AbstractValidator<FailProvisioningJobCommand>
{
    public FailProvisioningJobCommandValidator()
    {
        RuleFor(x => x.JobId)
            .NotEmpty().WithMessage("Job ID is required.");

        RuleFor(x => x.ErrorMessage)
            .NotEmpty().WithMessage("Error message is required.")
            .MaximumLength(2000).WithMessage("Error message must not exceed 2000 characters.");
    }
}
