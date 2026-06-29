using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Ticketing.Application.DTOs;

namespace Obss.Ticketing.Application.Commands.CloseTicket;

public sealed record CloseTicketCommand(Guid TicketId) : IRequest<Result<TicketDto>>;
