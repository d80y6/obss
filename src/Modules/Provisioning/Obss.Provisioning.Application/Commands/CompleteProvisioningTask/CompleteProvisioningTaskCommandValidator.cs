using FluentValidation;

namespace Obss.Provisioning.Application.Commands.CompleteProvisioningTask;

internal sealed class CompleteProvisioningTaskCommandValidator : AbstractValidator<CompleteProvisioningTaskCommand>
{
    public CompleteProvisioningTaskCommandValidator()
    {
        RuleFor(x => x.JobId)
            .NotEmpty().WithMessage("Job ID is required.");

        RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("Task ID is required.");
    }
}
