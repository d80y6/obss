using FluentValidation;

namespace Obss.Workflow.Application.Commands.CompleteWorkflowInstance;

internal sealed class CompleteWorkflowInstanceCommandValidator : AbstractValidator<CompleteWorkflowInstanceCommand>
{
    public CompleteWorkflowInstanceCommandValidator()
    {
        RuleFor(x => x.InstanceId)
            .NotEmpty().WithMessage("Instance ID is required.");
    }
}
