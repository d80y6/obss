using FluentValidation;

namespace Obss.Provisioning.Application.Commands.CreateProvisioningTemplate;

internal sealed class CreateProvisioningTemplateCommandValidator : AbstractValidator<CreateProvisioningTemplateCommand>
{
    public CreateProvisioningTemplateCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Template name is required.")
            .MaximumLength(200).WithMessage("Template name must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.")
            .When(x => x.Description is not null);

        RuleFor(x => x.ServiceType)
            .NotEmpty().WithMessage("Service type is required.")
            .MaximumLength(100).WithMessage("Service type must not exceed 100 characters.");

        RuleFor(x => x.Action)
            .NotEmpty().WithMessage("Action is required.")
            .MaximumLength(100).WithMessage("Action must not exceed 100 characters.");

        RuleFor(x => x.WorkflowDefinitionId)
            .NotEmpty().WithMessage("Workflow definition ID is required.");
    }
}
