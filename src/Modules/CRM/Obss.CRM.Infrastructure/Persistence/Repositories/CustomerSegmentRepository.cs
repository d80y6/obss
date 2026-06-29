using Microsoft.EntityFrameworkCore;
using Obss.CRM.Application.Abstractions;
using Obss.CRM.Domain.Entities;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.CRM.Infrastructure.Persistence.Repositories;

public sealed class CustomerSegmentRepository : EfRepository<CustomerSegment>, ICustomerSegmentRepository
{
    public CustomerSegmentRepository(CrmDbContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<CustomerSegment>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(s => s.IsActive)
            .OrderByDescending(s => s.Priority)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCustomerCountAsync(Guid segmentId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<CustomerSegmentAssignment>()
            .Where(a => a.SegmentId == segmentId)
            .CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CustomerSegmentAssignment>> GetAssignmentsAsync(Guid segmentId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<CustomerSegmentAssignment>()
            .Where(a => a.SegmentId == segmentId)
            .OrderByDescending(a => a.AssignedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsCustomerInSegmentAsync(Guid segmentId, Guid customerId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<CustomerSegmentAssignment>()
            .AnyAsync(a => a.SegmentId == segmentId && a.CustomerId == customerId, cancellationToken);
    }
}
