using FluentValidation;

namespace Obss.Notifications.Application.Commands.SendNotification;

public sealed class SendNotificationCommandValidator : AbstractValidator<SendNotificationCommand>
{
    public SendNotificationCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required.");

        RuleFor(x => x.NotificationType)
            .NotEmpty().WithMessage("Notification type is required.")
            .Must(v => v is "Email" or "SMS" or "InApp" or "Push")
            .WithMessage("Notification type must be Email, SMS, InApp, or Push.");

        RuleFor(x => x.Channel)
            .NotEmpty().WithMessage("Channel is required.");

        RuleFor(x => x.Recipient)
            .NotEmpty().WithMessage("Recipient is required.")
            .MaximumLength(500).WithMessage("Recipient must not exceed 500 characters.");

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Subject is required.")
            .MaximumLength(500).WithMessage("Subject must not exceed 500 characters.");

        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("Body is required.");

        RuleFor(x => x.Priority)
            .Must(v => v is null or "Low" or "Normal" or "High" or "Urgent")
            .WithMessage("Priority must be Low, Normal, High, or Urgent.")
            .When(x => x.Priority is not null);

        RuleFor(x => x.ReferenceType)
            .MaximumLength(100).WithMessage("Reference type must not exceed 100 characters.")
            .When(x => x.ReferenceType is not null);
    }
}
