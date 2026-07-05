using FluentValidation;

namespace Obss.Workflow.Application.Commands.ApplyWorkflowSla;

internal sealed class ApplyWorkflowSlaCommandValidator : AbstractValidator<ApplyWorkflowSlaCommand>
{
    public ApplyWorkflowSlaCommandValidator()
    {
        RuleFor(x => x.WorkflowInstanceId)
            .NotEmpty().WithMessage("Workflow instance ID is required.");

        RuleFor(x => x.WorkflowSlaId)
            .NotEmpty().WithMessage("Workflow SLA ID is required.");
    }
}
