using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Ticketing.Application.DTOs;

namespace Obss.Ticketing.Application.Commands.ResolveTicket;

public sealed record ResolveTicketCommand(
    Guid TicketId,
    string Resolution) : IRequest<Result<TicketDto>>;
