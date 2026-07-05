using FluentValidation;

namespace Obss.Provisioning.Application.Commands.StartProvisioningJob;

internal sealed class StartProvisioningJobCommandValidator : AbstractValidator<StartProvisioningJobCommand>
{
    public StartProvisioningJobCommandValidator()
    {
        RuleFor(x => x.JobId)
            .NotEmpty().WithMessage("Job ID is required.");
    }
}
