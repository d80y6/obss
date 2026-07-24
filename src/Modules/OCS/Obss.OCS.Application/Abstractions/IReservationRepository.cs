using Obss.OCS.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.OCS.Application.Abstractions;

public interface IReservationRepository : IRepository<Reservation>
{
    Task<IReadOnlyList<Reservation>> GetExpiredReservationsAsync(CancellationToken cancellationToken = default);
}
