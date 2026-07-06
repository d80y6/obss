using Microsoft.EntityFrameworkCore;
using Obss.Provisioning.Application.Abstractions;
using Obss.Provisioning.Domain.Entities;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.Provisioning.Infrastructure.Persistence.Repositories;

public sealed class ProvisioningJobRepository : EfRepository<ProvisioningJob>, IProvisioningJobRepository
{
    public ProvisioningJobRepository(ProvisioningDbContext context)
        : base(context)
    {
    }

    public async Task<ProvisioningJob?> GetByIdWithTasksAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(j => j.Tasks)
            .FirstOrDefaultAsync(j => j.Id == jobId, cancellationToken);
    }

    public async Task<IReadOnlyList<ProvisioningJob>> GetFilteredAsync(
        Guid? orderId,
        string? status,
        Guid? serviceId,
        int offset,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(j => j.Tasks)
            .AsQueryable();

        if (orderId.HasValue)
            query = query.Where(j => j.OrderId == orderId.Value);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(j => j.Status.ToString() == status);

        if (serviceId.HasValue)
            query = query.Where(j => j.ServiceId == serviceId.Value);

        query = query
            .OrderByDescending(j => j.CreatedAt)
            .Skip(offset)
            .Take(limit);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ProvisioningJob>> GetQueuedJobsAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(j => j.Tasks)
            .Where(j => j.Status == Domain.ValueObjects.JobStatus.Queued)
            .OrderBy(j => j.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
