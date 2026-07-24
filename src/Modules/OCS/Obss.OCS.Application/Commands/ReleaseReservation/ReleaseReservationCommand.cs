using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.OCS.Application.Commands.ReleaseReservation;

public sealed record ReleaseReservationCommand(Guid ReservationId) : IRequest<Result>;
