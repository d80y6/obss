using FluentValidation;

namespace Obss.Orders.Application.Commands.PatchProductOrder;

public sealed class PatchProductOrderCommandValidator : AbstractValidator<PatchProductOrderCommand>
{
    public PatchProductOrderCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Order ID is required.");

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes must not exceed 2000 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");

        RuleFor(x => x.Channel)
            .MaximumLength(50).WithMessage("Channel must not exceed 50 characters.");

        RuleFor(x => x.Priority)
            .MaximumLength(30).WithMessage("Priority must not exceed 30 characters.");

        RuleFor(x => x.NotificationContact)
            .MaximumLength(200).WithMessage("Notification contact must not exceed 200 characters.");

        RuleFor(x => x.ExternalId)
            .MaximumLength(100).WithMessage("External ID must not exceed 100 characters.");
    }
}
