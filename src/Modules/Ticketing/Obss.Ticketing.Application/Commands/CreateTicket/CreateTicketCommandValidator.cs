using FluentValidation;

namespace Obss.Ticketing.Application.Commands.CreateTicket;

public sealed class CreateTicketCommandValidator : AbstractValidator<CreateTicketCommand>
{
    public CreateTicketCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required.");

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");

        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("Customer name is required.")
            .MaximumLength(200).WithMessage("Customer name must not exceed 200 characters.");

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Subject is required.")
            .MaximumLength(500).WithMessage("Subject must not exceed 500 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(4000).WithMessage("Description must not exceed 4000 characters.");

        RuleFor(x => x.Priority)
            .NotEmpty().WithMessage("Priority is required.")
            .Must(v => Enum.TryParse<Domain.ValueObjects.TicketPriority>(v, true, out _))
            .WithMessage("Invalid priority value. Valid values: Low, Medium, High, Critical.");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required.")
            .Must(v => Enum.TryParse<Domain.ValueObjects.TicketCategory>(v, true, out _))
            .WithMessage("Invalid category value. Valid values: Billing, Technical, Account, ServiceRequest, Complaint, Other.");

        RuleFor(x => x.Source)
            .NotEmpty().WithMessage("Source is required.")
            .Must(v => Enum.TryParse<Domain.ValueObjects.TicketSource>(v, true, out _))
            .WithMessage("Invalid source value. Valid values: Portal, Email, Phone, Chat, API.");
    }
}
