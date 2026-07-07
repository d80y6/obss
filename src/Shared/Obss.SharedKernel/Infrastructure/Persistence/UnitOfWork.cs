using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.SharedKernel.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly IServiceProvider _serviceProvider;

    public UnitOfWork(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var dbContexts = _serviceProvider.GetServices<EfDbContext>();
        var logger = _serviceProvider.GetService<ILogger<UnitOfWork>>();
        var contextsWithChanges = dbContexts
            .Where(c => c.ChangeTracker.HasChanges())
            .ToList();

        if (contextsWithChanges.Count > 1)
        {
            var contextNames = string.Join(", ", contextsWithChanges.Select(c => c.GetType().Name));
            logger?.LogError(
                "UnitOfWork detected {Count} DbContexts with changes: {ContextNames}. " +
                "Each handler must save only its own module's context. " +
                "Cross-module operations must use integration events.",
                contextsWithChanges.Count, contextNames);

            throw new InvalidOperationException(
                $"UnitOfWork detected {contextsWithChanges.Count} DbContexts with pending changes: {contextNames}. " +
                $"Each command handler must save only its own module's context. " +
                "Cross-module operations must use integration events.");
        }

        var total = 0;
        foreach (var ctx in contextsWithChanges)
        {
            total += await ctx.SaveChangesAsync(cancellationToken);
        }

        return total;
    }

    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        var result = await SaveChangesAsync(cancellationToken);
        return result > 0;
    }
}