using FluentValidation;

namespace Obss.Notifications.Application.Commands.UpdateNotificationPreference;

internal sealed class UpdateNotificationPreferenceCommandValidator : AbstractValidator<UpdateNotificationPreferenceCommand>
{
    public UpdateNotificationPreferenceCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Preference ID is required.");

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");

        RuleFor(x => x.NotificationType)
            .NotEmpty().WithMessage("Notification type is required.")
            .MaximumLength(100).WithMessage("Notification type must not exceed 100 characters.");

        RuleFor(x => x.Channel)
            .NotEmpty().WithMessage("Channel is required.")
            .MaximumLength(50).WithMessage("Channel must not exceed 50 characters.");
    }
}
