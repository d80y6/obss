using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Domain.Specifications;

namespace Obss.SharedKernel.Infrastructure.Persistence;

public class EfRepository<T> : IRepository<T>
    where T : class
{
    protected readonly EfDbContext Context;
    protected readonly DbSet<T> DbSet;

    public EfRepository(EfDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public virtual IQueryable<T> GetQueryable() => DbSet.AsQueryable();

    public virtual async Task<T?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken = default)
        where TId : notnull
    {
        return await DbSet.FindAsync([id], cancellationToken);
    }

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.ToListAsync(cancellationToken);
    }

    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        var entry = await DbSet.AddAsync(entity, cancellationToken);
        return entry.Entity;
    }

    public virtual Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        Context.Entry(entity).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    public virtual Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        DbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.CountAsync(cancellationToken);
    }

    public virtual async Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(cancellationToken);
    }

    public virtual async Task<IReadOnlyList<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    public virtual async Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public virtual async Task<IReadOnlyList<T>> GetBySpecificationAsync(
        Specification<T> specification,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).ToListAsync(cancellationToken);
    }

    public virtual async Task<T?> GetBySpecificationSingleAsync(
        Specification<T> specification,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).SingleOrDefaultAsync(cancellationToken);
    }

    public virtual async Task<int> CountBySpecificationAsync(
        Specification<T> specification,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).CountAsync(cancellationToken);
    }

    public virtual async Task<bool> ExistsBySpecificationAsync(
        Specification<T> specification,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).AnyAsync(cancellationToken);
    }

    public virtual async Task<(IReadOnlyList<T> Items, int TotalCount)> GetPaginatedAsync(int offset, int limit, CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip(offset)
            .Take(limit)
            .ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    protected virtual IQueryable<T> ApplySpecification(Specification<T> specification)
    {
        return SpecificationEvaluator.GetQuery(DbSet.AsQueryable(), specification);
    }
}
