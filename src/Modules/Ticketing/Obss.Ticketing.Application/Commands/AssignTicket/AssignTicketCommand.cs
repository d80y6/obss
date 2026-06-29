using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Ticketing.Application.DTOs;

namespace Obss.Ticketing.Application.Commands.AssignTicket;

public sealed record AssignTicketCommand(
    Guid TicketId,
    string UserId,
    string? Group) : IRequest<Result<TicketDto>>;
