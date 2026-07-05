using FluentValidation;

namespace Obss.Ticketing.Application.Commands.AssignTicket;

internal sealed class AssignTicketCommandValidator : AbstractValidator<AssignTicketCommand>
{
    public AssignTicketCommandValidator()
    {
        RuleFor(x => x.TicketId)
            .NotEmpty().WithMessage("Ticket ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.")
            .MaximumLength(100).WithMessage("User ID must not exceed 100 characters.");
    }
}
