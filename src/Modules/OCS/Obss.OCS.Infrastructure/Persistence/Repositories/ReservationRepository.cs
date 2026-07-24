using Microsoft.EntityFrameworkCore;
using Obss.OCS.Application.Abstractions;
using Obss.OCS.Domain.Entities;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.OCS.Infrastructure.Persistence.Repositories;

public sealed class ReservationRepository : EfRepository<Reservation>, IReservationRepository
{
    public ReservationRepository(OcsDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Reservation>> GetExpiredReservationsAsync(CancellationToken cancellationToken = default)
    {
        return await GetQueryable()
            .Where(r => r.Status == ReservationStatus.Reserved && r.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }
}
