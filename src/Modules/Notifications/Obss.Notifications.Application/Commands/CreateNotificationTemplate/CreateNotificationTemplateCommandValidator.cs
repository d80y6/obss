using FluentValidation;

namespace Obss.Notifications.Application.Commands.CreateNotificationTemplate;

public sealed class CreateNotificationTemplateCommandValidator
    : AbstractValidator<CreateNotificationTemplateCommand>
{
    public CreateNotificationTemplateCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.NotificationType)
            .NotEmpty().WithMessage("Notification type is required.")
            .Must(v => v is "Email" or "SMS" or "InApp" or "Push")
            .WithMessage("Notification type must be Email, SMS, InApp, or Push.");

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Subject is required.")
            .MaximumLength(500).WithMessage("Subject must not exceed 500 characters.");

        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("Body is required.");
    }
}
