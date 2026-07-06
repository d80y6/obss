using Mapster;
using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Ticketing.Application.Abstractions;
using Obss.Ticketing.Application.DTOs;

namespace Obss.Ticketing.Application.Queries.GetTickets;

public sealed class GetTicketsQueryHandler : IRequestHandler<GetTicketsQuery, Result<IReadOnlyList<TicketSummaryDto>>>
{
    private readonly ITicketRepository _ticketRepository;

    public GetTicketsQueryHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<Result<IReadOnlyList<TicketSummaryDto>>> Handle(GetTicketsQuery request, CancellationToken cancellationToken)
    {
        var tickets = await _ticketRepository.GetFilteredAsync(
            request.TenantId,
            request.Status,
            request.Priority,
            request.Category,
            request.CustomerId,
            request.AssignedTo,
            request.FromDate,
            request.ToDate,
            request.Offset,
            request.Limit,
            cancellationToken);

        var result = tickets.Adapt<List<TicketSummaryDto>>();
        return Result.Success<IReadOnlyList<TicketSummaryDto>>(result);
    }
}
