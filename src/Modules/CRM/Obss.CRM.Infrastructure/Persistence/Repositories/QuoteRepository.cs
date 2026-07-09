using Microsoft.EntityFrameworkCore;
using Obss.CRM.Application.Abstractions;
using Obss.CRM.Domain.Entities;
using Obss.CRM.Infrastructure.Persistence;

namespace Obss.CRM.Infrastructure.Persistence.Repositories;

public sealed class QuoteRepository : IQuoteRepository
{
    private readonly CrmDbContext _context;

    public QuoteRepository(CrmDbContext context) => _context = context;

    public IQueryable<Quote> GetQueryable() => _context.Set<Quote>().AsQueryable();

    public async Task<Quote?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken = default)
        where TId : notnull
    {
        if (id is Guid guid)
            return await _context.Set<Quote>().FirstOrDefaultAsync(q => q.Id == guid, cancellationToken);
        return null;
    }

    public async Task<Quote?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Set<Quote>().FirstOrDefaultAsync(q => q.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Quote>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Set<Quote>().ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Quote>> GetListAsync(CancellationToken cancellationToken = default)
        => await _context.Set<Quote>().ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Quote>> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
        => await _context.Set<Quote>().Where(q => q.CustomerId == customerId).ToListAsync(cancellationToken);

    public async Task<Quote> AddAsync(Quote quote, CancellationToken cancellationToken = default)
    {
        await _context.Set<Quote>().AddAsync(quote, cancellationToken);
        return quote;
    }

    public Task UpdateAsync(Quote quote, CancellationToken cancellationToken = default)
    {
        _context.Set<Quote>().Update(quote);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Quote quote, CancellationToken cancellationToken = default)
    {
        _context.Set<Quote>().Remove(quote);
        return Task.CompletedTask;
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
        => _context.Set<Quote>().CountAsync(cancellationToken);

    public Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
        => _context.Set<Quote>().AnyAsync(cancellationToken);

    public async Task<(IReadOnlyList<Quote> Items, int TotalCount)> GetPaginatedAsync(int offset, int limit, CancellationToken cancellationToken = default)
    {
        var query = _context.Set<Quote>().AsQueryable();
        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip(offset).Take(limit).ToListAsync(cancellationToken);
        return (items, total);
    }
}
