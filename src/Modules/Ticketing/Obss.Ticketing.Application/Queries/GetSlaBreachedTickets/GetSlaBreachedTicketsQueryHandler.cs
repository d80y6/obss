using Mapster;
using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Ticketing.Application.Abstractions;
using Obss.Ticketing.Application.DTOs;

namespace Obss.Ticketing.Application.Queries.GetSlaBreachedTickets;

public sealed class GetSlaBreachedTicketsQueryHandler : IRequestHandler<GetSlaBreachedTicketsQuery, Result<IReadOnlyList<TicketSummaryDto>>>
{
    private readonly ITicketRepository _ticketRepository;

    public GetSlaBreachedTicketsQueryHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<Result<IReadOnlyList<TicketSummaryDto>>> Handle(GetSlaBreachedTicketsQuery request, CancellationToken cancellationToken)
    {
        var tickets = await _ticketRepository.GetSlaBreachedTicketsAsync(request.TenantId, cancellationToken);
        var result = tickets.Adapt<List<TicketSummaryDto>>();
        return Result.Success<IReadOnlyList<TicketSummaryDto>>(result);
    }
}
