using Microsoft.EntityFrameworkCore;
using Obss.Rating.Application.Abstractions;
using Obss.Rating.Domain.Entities;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.Rating.Infrastructure.Persistence.Repositories;

public sealed class CdrRecordRepository : EfRepository<CdrRecord>, ICdrRecordRepository
{
    public CdrRecordRepository(RatingDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<CdrRecord>> GetByVendorAsync(string vendor, CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(r => r.Vendor == vendor).OrderByDescending(r => r.ReceivedAt).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CdrRecord>> GetByStatusAsync(string status, CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(r => r.Status == status).OrderByDescending(r => r.ReceivedAt).ToListAsync(cancellationToken);
    }

    public async Task<CdrRecord?> GetByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(r => r.CorrelationId == correlationId, cancellationToken);
    }
}
