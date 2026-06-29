using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Ticketing.Application.DTOs;

namespace Obss.Ticketing.Application.Commands.ApplySlaToTicket;

public sealed record ApplySlaToTicketCommand(
    Guid TicketId,
    Guid SlaDefinitionId) : IRequest<Result<TicketDto>>;
