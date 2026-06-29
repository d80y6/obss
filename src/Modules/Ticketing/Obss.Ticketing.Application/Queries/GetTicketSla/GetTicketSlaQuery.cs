using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Ticketing.Application.DTOs;

namespace Obss.Ticketing.Application.Queries.GetTicketSla;

public sealed record GetTicketSlaQuery(Guid TicketId) : IRequest<Result<TicketSlaDto>>;
