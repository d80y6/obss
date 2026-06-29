using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Ticketing.Application.DTOs;

namespace Obss.Ticketing.Application.Queries.GetTickets;

public sealed record GetTicketsQuery(
    string? TenantId,
    string? Status,
    string? Priority,
    string? Category,
    Guid? CustomerId,
    string? AssignedTo,
    DateTime? FromDate,
    DateTime? ToDate,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<IReadOnlyList<TicketSummaryDto>>>;
