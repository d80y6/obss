using FluentValidation;

namespace Obss.IAM.Application.Commands.CreateTenant;

internal sealed class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug is required.")
            .MaximumLength(100).WithMessage("Slug must not exceed 100 characters.")
            .Matches(@"^[a-z0-9-]+$").WithMessage("Slug can only contain lowercase letters, numbers, and hyphens.");

        RuleFor(x => x.ConnectionString)
            .MaximumLength(500).WithMessage("Connection string must not exceed 500 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.ConnectionString));

        RuleFor(x => x.Settings)
            .MaximumLength(4000).WithMessage("Settings must not exceed 4000 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Settings));

        RuleFor(x => x.AdminUsername)
            .NotEmpty().WithMessage("Admin username is required.")
            .MinimumLength(3).WithMessage("Admin username must be at least 3 characters.")
            .MaximumLength(50).WithMessage("Admin username must not exceed 50 characters.");

        RuleFor(x => x.AdminEmail)
            .NotEmpty().WithMessage("Admin email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(256).WithMessage("Admin email must not exceed 256 characters.");

        RuleFor(x => x.AdminFirstName)
            .NotEmpty().WithMessage("Admin first name is required.")
            .MaximumLength(100).WithMessage("Admin first name must not exceed 100 characters.");

        RuleFor(x => x.AdminLastName)
            .NotEmpty().WithMessage("Admin last name is required.")
            .MaximumLength(100).WithMessage("Admin last name must not exceed 100 characters.");
    }
}
