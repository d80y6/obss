using FluentValidation;

namespace Obss.Ticketing.Application.Commands.EscalateTicket;

public sealed class EscalateTicketCommandValidator : AbstractValidator<EscalateTicketCommand>
{
    public EscalateTicketCommandValidator()
    {
        RuleFor(x => x.TicketId)
            .NotEmpty().WithMessage("Ticket ID is required.");

        RuleFor(x => x.EscalatedBy)
            .NotEmpty().WithMessage("Escalated by is required.")
            .MaximumLength(100).WithMessage("Escalated by must not exceed 100 characters.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Escalation reason is required.")
            .MaximumLength(1000).WithMessage("Reason must not exceed 1000 characters.");
    }
}
