using Microsoft.EntityFrameworkCore;
using Obss.CRM.Application.Abstractions;
using Obss.CRM.Domain.Entities;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.CRM.Infrastructure.Persistence.Repositories;

public sealed class CustomerRepository : EfRepository<Customer>, ICustomerRepository
{
    public CustomerRepository(CrmDbContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<Customer>> GetFilteredAsync(
        string? tenantId,
        string? status,
        string? customerType,
        string? searchTerm,
        int offset,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(tenantId))
        {
            query = query.Where(c => c.TenantId == tenantId);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(c => c.Status.ToString() == status);
        }

        if (!string.IsNullOrWhiteSpace(customerType))
        {
            query = query.Where(c => c.CustomerType.ToString() == customerType);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(c =>
                c.DisplayName.Contains(searchTerm) ||
                c.CompanyName!.Contains(searchTerm) ||
                c.Email.Value.Contains(searchTerm) ||
                c.TaxNumber!.Contains(searchTerm));
        }

        query = query
            .OrderBy(c => c.DisplayName)
            .Skip(offset)
            .Take(limit);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Contact>> GetContactsByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Contact>()
            .Where(c => c.CustomerId == customerId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CustomerNote>> GetNotesByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<CustomerNote>()
            .Where(n => n.CustomerId == customerId)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetFilteredCountAsync(
        string? tenantId,
        string? status,
        string? customerType,
        string? searchTerm,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(tenantId))
            query = query.Where(c => c.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(c => c.Status.ToString() == status);

        if (!string.IsNullOrWhiteSpace(customerType))
            query = query.Where(c => c.CustomerType.ToString() == customerType);

        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(c =>
                c.DisplayName.Contains(searchTerm) ||
                c.CompanyName!.Contains(searchTerm) ||
                c.Email.Value.Contains(searchTerm) ||
                c.TaxNumber!.Contains(searchTerm));

        return await query.CountAsync(cancellationToken);
    }
}
