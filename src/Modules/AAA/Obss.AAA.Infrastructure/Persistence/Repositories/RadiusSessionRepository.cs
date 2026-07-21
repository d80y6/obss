using Microsoft.EntityFrameworkCore;
using Obss.AAA.Application.Abstractions;
using Obss.AAA.Domain.Entities;
using Obss.AAA.Domain.ValueObjects;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.AAA.Infrastructure.Persistence.Repositories;

public sealed class RadiusSessionRepository : EfRepository<RadiusSession>, IRadiusSessionRepository
{
    public RadiusSessionRepository(AaaDbContext context) : base(context)
    {
    }

    public async Task<RadiusSession?> GetBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(s => s.SessionId == sessionId, cancellationToken);
    }

    public async Task<IReadOnlyList<RadiusSession>> GetActiveSessionsAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(s => s.SessionStatus == SessionStatus.Active).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RadiusSession>> GetActiveSessionsByNasAsync(Guid nasId, CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(s => s.SessionStatus == SessionStatus.Active && s.NasId == nasId).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RadiusSession>> GetSessionsByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(s => s.Username == username).OrderByDescending(s => s.StartedAt).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RadiusSession>> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(s => s.Username == username).OrderByDescending(s => s.StartedAt).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RadiusSession>> GetByNasIdAsync(string nasId, CancellationToken cancellationToken = default)
    {
        var guid = Guid.Parse(nasId);
        return await DbSet.Where(s => s.NasId == guid).OrderByDescending(s => s.StartedAt).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RadiusSession>> GetSessionsByStatusAsync(SessionStatus status, CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(s => s.SessionStatus == status).OrderByDescending(s => s.StartedAt).ToListAsync(cancellationToken);
    }

    public async Task<int> CountActiveSessionsAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.CountAsync(s => s.SessionStatus == SessionStatus.Active, cancellationToken);
    }
}
