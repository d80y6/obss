using FluentValidation;

namespace Obss.Notifications.Application.Commands.UpdateTemplate;

internal sealed class UpdateTemplateCommandValidator : AbstractValidator<UpdateTemplateCommand>
{
    public UpdateTemplateCommandValidator()
    {
        RuleFor(x => x.TemplateId)
            .NotEmpty().WithMessage("Template ID is required.");

        RuleFor(x => x.Name)
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.")
            .When(x => x.Name is not null);

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.")
            .When(x => x.Description is not null);

        RuleFor(x => x.Subject)
            .MaximumLength(500).WithMessage("Subject must not exceed 500 characters.")
            .When(x => x.Subject is not null);

        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("Body must not be empty when provided.")
            .When(x => x.Body is not null);
    }
}
