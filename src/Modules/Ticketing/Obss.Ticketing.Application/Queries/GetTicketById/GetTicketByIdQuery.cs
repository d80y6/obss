using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Ticketing.Application.DTOs;

namespace Obss.Ticketing.Application.Queries.GetTicketById;

public sealed record GetTicketByIdQuery(Guid TicketId) : IRequest<Result<TicketDto>>;
