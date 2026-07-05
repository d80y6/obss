using FluentValidation;

namespace Obss.Provisioning.Application.Commands.RetryProvisioningJob;

internal sealed class RetryProvisioningJobCommandValidator : AbstractValidator<RetryProvisioningJobCommand>
{
    public RetryProvisioningJobCommandValidator()
    {
        RuleFor(x => x.JobId)
            .NotEmpty().WithMessage("Job ID is required.");
    }
}
