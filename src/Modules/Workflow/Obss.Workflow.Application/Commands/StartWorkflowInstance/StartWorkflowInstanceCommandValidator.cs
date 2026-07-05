using FluentValidation;

namespace Obss.Workflow.Application.Commands.StartWorkflowInstance;

internal sealed class StartWorkflowInstanceCommandValidator : AbstractValidator<StartWorkflowInstanceCommand>
{
    public StartWorkflowInstanceCommandValidator()
    {
        RuleFor(x => x.WorkflowDefinitionId)
            .NotEmpty().WithMessage("Workflow definition ID is required.");

        RuleFor(x => x.TriggerEntityType)
            .NotEmpty().WithMessage("Trigger entity type is required.")
            .MaximumLength(100).WithMessage("Trigger entity type must not exceed 100 characters.");

        RuleFor(x => x.TriggerEntityId)
            .NotEmpty().WithMessage("Trigger entity ID is required.");

        RuleFor(x => x.CreatedBy)
            .NotEmpty().WithMessage("Created by is required.")
            .MaximumLength(100).WithMessage("Created by must not exceed 100 characters.");
    }
}
