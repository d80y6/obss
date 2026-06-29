using FluentValidation;

namespace Obss.Ticketing.Application.Commands.ApplySlaToTicket;

public sealed class ApplySlaToTicketCommandValidator : AbstractValidator<ApplySlaToTicketCommand>
{
    public ApplySlaToTicketCommandValidator()
    {
        RuleFor(x => x.TicketId)
            .NotEmpty().WithMessage("Ticket ID is required.");

        RuleFor(x => x.SlaDefinitionId)
            .NotEmpty().WithMessage("SLA Definition ID is required.");
    }
}
