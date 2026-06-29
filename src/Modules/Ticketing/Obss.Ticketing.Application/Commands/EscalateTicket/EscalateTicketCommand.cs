using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Ticketing.Application.DTOs;

namespace Obss.Ticketing.Application.Commands.EscalateTicket;

public sealed record EscalateTicketCommand(
    Guid TicketId,
    string EscalatedBy,
    string Reason) : IRequest<Result<TicketDto>>;
