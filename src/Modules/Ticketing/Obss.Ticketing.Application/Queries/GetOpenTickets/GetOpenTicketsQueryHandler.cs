using Mapster;
using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Ticketing.Application.Abstractions;
using Obss.Ticketing.Application.DTOs;

namespace Obss.Ticketing.Application.Queries.GetOpenTickets;

public sealed class GetOpenTicketsQueryHandler : IRequestHandler<GetOpenTicketsQuery, Result<IReadOnlyList<TicketSummaryDto>>>
{
    private readonly ITicketRepository _ticketRepository;

    public GetOpenTicketsQueryHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<Result<IReadOnlyList<TicketSummaryDto>>> Handle(GetOpenTicketsQuery request, CancellationToken cancellationToken)
    {
        var tickets = await _ticketRepository.GetOpenTicketsAsync(request.TenantId, cancellationToken);
        var result = tickets.Adapt<List<TicketSummaryDto>>();
        return Result.Success<IReadOnlyList<TicketSummaryDto>>(result);
    }
}
