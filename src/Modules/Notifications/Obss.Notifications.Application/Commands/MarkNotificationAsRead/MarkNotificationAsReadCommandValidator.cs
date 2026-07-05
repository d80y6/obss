using FluentValidation;

namespace Obss.Notifications.Application.Commands.MarkNotificationAsRead;

internal sealed class MarkNotificationAsReadCommandValidator : AbstractValidator<MarkNotificationAsReadCommand>
{
    public MarkNotificationAsReadCommandValidator()
    {
        RuleFor(x => x.NotificationId)
            .NotEmpty().WithMessage("Notification ID is required.");
    }
}
