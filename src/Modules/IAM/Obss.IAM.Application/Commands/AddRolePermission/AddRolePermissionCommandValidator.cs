using FluentValidation;

namespace Obss.IAM.Application.Commands.AddRolePermission;

internal sealed class AddRolePermissionCommandValidator : AbstractValidator<AddRolePermissionCommand>
{
    public AddRolePermissionCommandValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("Role ID is required.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(100).WithMessage("Code must not exceed 100 characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.Module)
            .NotEmpty().WithMessage("Module is required.")
            .MaximumLength(100).WithMessage("Module must not exceed 100 characters.");

        RuleFor(x => x.Resource)
            .NotEmpty().WithMessage("Resource is required.")
            .MaximumLength(100).WithMessage("Resource must not exceed 100 characters.");

        RuleFor(x => x.Action)
            .NotEmpty().WithMessage("Action is required.")
            .MaximumLength(50).WithMessage("Action must not exceed 50 characters.");
    }
}
