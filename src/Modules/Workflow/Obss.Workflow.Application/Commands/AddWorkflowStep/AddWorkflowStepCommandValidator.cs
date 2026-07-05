using FluentValidation;

namespace Obss.Workflow.Application.Commands.AddWorkflowStep;

internal sealed class AddWorkflowStepCommandValidator : AbstractValidator<AddWorkflowStepCommand>
{
    public AddWorkflowStepCommandValidator()
    {
        RuleFor(x => x.WorkflowDefinitionId)
            .NotEmpty().WithMessage("Workflow definition ID is required.");

        RuleFor(x => x.StepNumber)
            .GreaterThan(0).WithMessage("Step number must be greater than zero.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Step name is required.")
            .MaximumLength(200).WithMessage("Step name must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.")
            .When(x => x.Description is not null);

        RuleFor(x => x.StepType)
            .NotEmpty().WithMessage("Step type is required.")
            .MaximumLength(100).WithMessage("Step type must not exceed 100 characters.");

        RuleFor(x => x.HandlerType)
            .MaximumLength(500).WithMessage("Handler type must not exceed 500 characters.")
            .When(x => x.HandlerType is not null);

        RuleFor(x => x.Configuration)
            .MaximumLength(4000).WithMessage("Configuration must not exceed 4000 characters.")
            .When(x => x.Configuration is not null);

        RuleFor(x => x.Timeout)
            .GreaterThanOrEqualTo(0).WithMessage("Timeout must be zero or greater.");

        RuleFor(x => x.RetryCount)
            .GreaterThanOrEqualTo(0).WithMessage("Retry count must be zero or greater.");

        RuleFor(x => x.RetryDelaySeconds)
            .GreaterThanOrEqualTo(0).WithMessage("Retry delay seconds must be zero or greater.");
    }
}
