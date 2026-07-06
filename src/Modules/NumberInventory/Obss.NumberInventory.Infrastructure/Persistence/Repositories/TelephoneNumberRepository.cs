using Microsoft.EntityFrameworkCore;
using Obss.NumberInventory.Application.Abstractions;
using Obss.NumberInventory.Domain.Entities;
using Obss.NumberInventory.Domain.ValueObjects;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.NumberInventory.Infrastructure.Persistence.Repositories;

public sealed class TelephoneNumberRepository : EfRepository<TelephoneNumber>, ITelephoneNumberRepository
{
    public TelephoneNumberRepository(NumberDbContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<TelephoneNumber>> GetAvailableNumbersAsync(
        NumberType? numberType,
        string? prefix,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(n => n.Status == NumberStatus.Available);

        if (numberType.HasValue)
        {
            query = query.Where(n => n.NumberType == numberType.Value);
        }

        if (!string.IsNullOrWhiteSpace(prefix))
        {
            query = query.Where(n => n.Number.StartsWith(prefix));
        }

        return await query
            .OrderBy(n => n.Number)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TelephoneNumber>> GetByCustomerAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(n => n.CustomerId == customerId)
            .OrderBy(n => n.Number)
            .ToListAsync(cancellationToken);
    }

    public async Task<TelephoneNumber?> GetByNumberAsync(
        string number,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(n => n.Number == number, cancellationToken);
    }

    public async Task<IReadOnlyList<TelephoneNumber>> SearchNumbersAsync(
        string? prefix,
        NumberStatus? status,
        NumberType? type,
        int offset,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(prefix))
        {
            query = query.Where(n => n.Number.StartsWith(prefix));
        }

        if (status.HasValue)
        {
            query = query.Where(n => n.Status == status.Value);
        }

        if (type.HasValue)
        {
            query = query.Where(n => n.NumberType == type.Value);
        }

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip(offset)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
}
