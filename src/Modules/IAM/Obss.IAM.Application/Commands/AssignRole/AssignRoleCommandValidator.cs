using FluentValidation;

namespace Obss.IAM.Application.Commands.AssignRole;

internal sealed class AssignRoleCommandValidator : AbstractValidator<AssignRoleCommand>
{
    public AssignRoleCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("Role ID is required.");

        RuleFor(x => x.AssignedBy)
            .NotEmpty().WithMessage("Assigned by is required.");
    }
}
