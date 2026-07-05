using FluentValidation;

namespace Obss.Ticketing.Application.Commands.ResolveTicket;

internal sealed class ResolveTicketCommandValidator : AbstractValidator<ResolveTicketCommand>
{
    public ResolveTicketCommandValidator()
    {
        RuleFor(x => x.TicketId)
            .NotEmpty().WithMessage("Ticket ID is required.");

        RuleFor(x => x.Resolution)
            .NotEmpty().WithMessage("Resolution is required.")
            .MaximumLength(2000).WithMessage("Resolution must not exceed 2000 characters.");
    }
}
