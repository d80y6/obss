using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Ticketing.Application.DTOs;

namespace Obss.Ticketing.Application.Queries.GetOpenTickets;

public sealed record GetOpenTicketsQuery(string? TenantId) : IRequest<Result<IReadOnlyList<TicketSummaryDto>>>;
