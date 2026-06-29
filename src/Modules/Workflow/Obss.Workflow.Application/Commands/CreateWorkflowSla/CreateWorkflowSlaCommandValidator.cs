using FluentValidation;

namespace Obss.Workflow.Application.Commands.CreateWorkflowSla;

public sealed class CreateWorkflowSlaCommandValidator : AbstractValidator<CreateWorkflowSlaCommand>
{
    public CreateWorkflowSlaCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.WorkflowDefinitionId)
            .NotEmpty().WithMessage("Workflow definition ID is required.");

        RuleFor(x => x.TargetDurationMinutes)
            .GreaterThan(0).WithMessage("Target duration must be greater than 0.");

        RuleFor(x => x.WarningThresholdPercent)
            .InclusiveBetween(0, 1).WithMessage("Warning threshold must be between 0 and 1.");
    }
}
