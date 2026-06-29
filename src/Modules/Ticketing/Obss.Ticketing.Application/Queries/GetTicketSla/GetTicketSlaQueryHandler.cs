using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Ticketing.Application.Abstractions;
using Obss.Ticketing.Application.DTOs;

namespace Obss.Ticketing.Application.Queries.GetTicketSla;

public sealed class GetTicketSlaQueryHandler : IRequestHandler<GetTicketSlaQuery, Result<TicketSlaDto>>
{
    private readonly ITicketRepository _ticketRepository;

    public GetTicketSlaQueryHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<Result<TicketSlaDto>> Handle(GetTicketSlaQuery request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken);

        if (ticket is null)
            return Result.Failure<TicketSlaDto>(Error.NotFound("Ticket", request.TicketId));

        var dto = new TicketSlaDto(
            ticket.SlaDefinitionId,
            string.Empty,
            ticket.SlaDeadline,
            ticket.SlaResponseDeadline,
            ticket.SlaBreachedAt,
            ticket.IsSlaBreached,
            ticket.Status.ToString());

        return Result.Success(dto);
    }
}
