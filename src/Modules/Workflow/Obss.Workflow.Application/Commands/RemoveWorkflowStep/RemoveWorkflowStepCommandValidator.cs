using FluentValidation;

namespace Obss.Workflow.Application.Commands.RemoveWorkflowStep;

internal sealed class RemoveWorkflowStepCommandValidator : AbstractValidator<RemoveWorkflowStepCommand>
{
    public RemoveWorkflowStepCommandValidator()
    {
        RuleFor(x => x.WorkflowDefinitionId)
            .NotEmpty().WithMessage("Workflow definition ID is required.");

        RuleFor(x => x.StepId)
            .NotEmpty().WithMessage("Step ID is required.");
    }
}
