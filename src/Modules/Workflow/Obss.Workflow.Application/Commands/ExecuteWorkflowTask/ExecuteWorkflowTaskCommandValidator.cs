using FluentValidation;

namespace Obss.Workflow.Application.Commands.ExecuteWorkflowTask;

internal sealed class ExecuteWorkflowTaskCommandValidator : AbstractValidator<ExecuteWorkflowTaskCommand>
{
    public ExecuteWorkflowTaskCommandValidator()
    {
        RuleFor(x => x.InstanceId)
            .NotEmpty().WithMessage("Instance ID is required.");

        RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("Task ID is required.");
    }
}
