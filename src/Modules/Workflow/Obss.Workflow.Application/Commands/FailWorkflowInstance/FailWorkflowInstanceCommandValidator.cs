using FluentValidation;

namespace Obss.Workflow.Application.Commands.FailWorkflowInstance;

internal sealed class FailWorkflowInstanceCommandValidator : AbstractValidator<FailWorkflowInstanceCommand>
{
    public FailWorkflowInstanceCommandValidator()
    {
        RuleFor(x => x.InstanceId)
            .NotEmpty().WithMessage("Instance ID is required.");

        RuleFor(x => x.Error)
            .NotEmpty().WithMessage("Error message is required.")
            .MaximumLength(2000).WithMessage("Error message must not exceed 2000 characters.");
    }
}
