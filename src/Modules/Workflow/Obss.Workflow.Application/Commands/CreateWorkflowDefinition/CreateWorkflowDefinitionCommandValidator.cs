using FluentValidation;

namespace Obss.Workflow.Application.Commands.CreateWorkflowDefinition;

public sealed class CreateWorkflowDefinitionCommandValidator : AbstractValidator<CreateWorkflowDefinitionCommand>
{
    public CreateWorkflowDefinitionCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required.")
            .Must(c => Enum.TryParse<Domain.ValueObjects.WorkflowCategory>(c, true, out _))
            .WithMessage("Invalid workflow category.");
    }
}
