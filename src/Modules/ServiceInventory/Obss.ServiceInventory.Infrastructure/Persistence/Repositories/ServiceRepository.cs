using Microsoft.EntityFrameworkCore;
using Obss.ServiceInventory.Application.Abstractions;
using Obss.ServiceInventory.Domain.Entities;
using Obss.ServiceInventory.Domain.ValueObjects;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.ServiceInventory.Infrastructure.Persistence.Repositories;

public sealed class ServiceRepository : EfRepository<Service>, IServiceRepository
{
    public ServiceRepository(ServiceDbContext context)
        : base(context)
    {
    }

    public async Task<Service?> GetByIdWithResourcesAsync(Guid serviceId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(s => s.Resources)
            .FirstOrDefaultAsync(s => s.Id == serviceId, cancellationToken);
    }

    public async Task<IReadOnlyList<Service>> GetFilteredAsync(
        Guid? customerId,
        ServiceType? serviceType,
        ServiceStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();

        if (customerId.HasValue)
            query = query.Where(s => s.CustomerId == customerId.Value);

        if (serviceType.HasValue)
            query = query.Where(s => s.ServiceType == serviceType.Value);

        if (status.HasValue)
            query = query.Where(s => s.Status == status.Value);

        query = query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<int> CountFilteredAsync(
        Guid? customerId,
        ServiceType? serviceType,
        ServiceStatus? status,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();

        if (customerId.HasValue)
            query = query.Where(s => s.CustomerId == customerId.Value);

        if (serviceType.HasValue)
            query = query.Where(s => s.ServiceType == serviceType.Value);

        if (status.HasValue)
            query = query.Where(s => s.Status == status.Value);

        return await query.CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Service>> GetBySubscriptionAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(s => s.SubscriptionId == subscriptionId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Service?> GetByIdentifierAsync(string identifier, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(s => s.ServiceIdentifier == identifier, cancellationToken);
    }
}
