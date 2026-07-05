using FluentValidation;

namespace Obss.Ticketing.Application.Commands.CloseTicket;

internal sealed class CloseTicketCommandValidator : AbstractValidator<CloseTicketCommand>
{
    public CloseTicketCommandValidator()
    {
        RuleFor(x => x.TicketId)
            .NotEmpty().WithMessage("Ticket ID is required.");
    }
}
