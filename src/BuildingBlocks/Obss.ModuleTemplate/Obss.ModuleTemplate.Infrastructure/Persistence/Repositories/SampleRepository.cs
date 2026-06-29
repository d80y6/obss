using Microsoft.EntityFrameworkCore;
using Obss.ModuleTemplate.Application.Abstractions;
using Obss.ModuleTemplate.Domain.Entities;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.ModuleTemplate.Infrastructure.Persistence.Repositories;

public sealed class SampleRepository : EfRepository<SampleAggregate>, ISampleRepository
{
    public SampleRepository(SampleDbContext context)
        : base(context)
    {
    }

    public async Task<SampleAggregate?> GetByNameAsync(string name, string tenantId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(s => s.Name == name && s.TenantId == tenantId, cancellationToken);
    }

    public async Task<IReadOnlyList<SampleAggregate>> GetFilteredAsync(
        string? tenantId,
        bool? isActive,
        string? searchTerm,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(tenantId))
            query = query.Where(s => s.TenantId == tenantId);

        if (isActive.HasValue)
            query = query.Where(s => s.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(s =>
                s.Name.Contains(searchTerm) ||
                (s.Description != null && s.Description.Contains(searchTerm)));

        query = query
            .OrderBy(s => s.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        return await query.ToListAsync(cancellationToken);
    }
}
