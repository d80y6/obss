using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Ticketing.Application.DTOs;

namespace Obss.Ticketing.Application.Commands.CreateTicket;

public sealed record CreateTicketCommand(
    string TenantId,
    Guid CustomerId,
    string CustomerName,
    string Subject,
    string Description,
    string Priority,
    string Category,
    string Source) : IRequest<Result<TicketDto>>;
