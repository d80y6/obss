using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Ticketing.Application.DTOs;

namespace Obss.Ticketing.Application.Queries.GetSlaBreachedTickets;

public sealed record GetSlaBreachedTicketsQuery(string? TenantId) : IRequest<Result<IReadOnlyList<TicketSummaryDto>>>;
