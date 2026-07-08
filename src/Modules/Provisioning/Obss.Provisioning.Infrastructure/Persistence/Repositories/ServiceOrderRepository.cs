using Microsoft.EntityFrameworkCore;
using Obss.Provisioning.Application.Abstractions;
using Obss.Provisioning.Domain.Entities;
using Obss.Provisioning.Infrastructure.Persistence;
using Obss.SharedKernel.Domain.Specifications;

namespace Obss.Provisioning.Infrastructure.Persistence.Repositories;

public sealed class ServiceOrderRepository : IServiceOrderRepository
{
    private readonly ProvisioningDbContext _context;

    public ServiceOrderRepository(ProvisioningDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceOrder?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Set<ServiceOrder>()
            .FirstOrDefaultAsync(o => o.Id == id, ct);
    }

    public async Task<List<ServiceOrder>> GetListAsync(Specification<ServiceOrder>? spec = null, CancellationToken ct = default)
    {
        var query = _context.Set<ServiceOrder>().AsQueryable();
        if (spec is not null)
            query = query.Where(spec.ToExpression());
        return await query.ToListAsync(ct);
    }

    public async Task AddAsync(ServiceOrder order, CancellationToken ct = default)
    {
        await _context.Set<ServiceOrder>().AddAsync(order, ct);
    }

    public Task UpdateAsync(ServiceOrder order, CancellationToken ct = default)
    {
        _context.Set<ServiceOrder>().Update(order);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(ServiceOrder order, CancellationToken ct = default)
    {
        _context.Set<ServiceOrder>().Remove(order);
        return Task.CompletedTask;
    }

    public async Task<int> CountAsync(Specification<ServiceOrder>? spec = null, CancellationToken ct = default)
    {
        var query = _context.Set<ServiceOrder>().AsQueryable();
        if (spec is not null)
            query = query.Where(spec.ToExpression());
        return await query.CountAsync(ct);
    }
}