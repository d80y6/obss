using FluentValidation;

namespace Obss.Notifications.Application.Commands.SendNotificationFromTemplate;

internal sealed class SendNotificationFromTemplateCommandValidator : AbstractValidator<SendNotificationFromTemplateCommand>
{
    public SendNotificationFromTemplateCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required.");

        RuleFor(x => x.TemplateId)
            .NotEmpty().WithMessage("Template ID is required.");

        RuleFor(x => x.Channel)
            .NotEmpty().WithMessage("Channel is required.")
            .MaximumLength(50).WithMessage("Channel must not exceed 50 characters.");

        RuleFor(x => x.Recipient)
            .NotEmpty().WithMessage("Recipient is required.")
            .MaximumLength(500).WithMessage("Recipient must not exceed 500 characters.");

        RuleFor(x => x.ReferenceType)
            .MaximumLength(100).WithMessage("Reference type must not exceed 100 characters.")
            .When(x => x.ReferenceType is not null);
    }
}
