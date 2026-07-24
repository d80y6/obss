using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.OCS.Application.Commands.DebitReservation;

public sealed record DebitReservationCommand(Guid ReservationId) : IRequest<Result>;

public sealed record DebitReservationResponse(Guid ReservationId, decimal Amount, string Currency, decimal RemainingBalance);
