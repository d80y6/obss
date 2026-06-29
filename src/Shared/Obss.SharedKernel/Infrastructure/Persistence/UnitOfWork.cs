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
        var total = 0;
        var hasErrors = false;

        foreach (var ctx in dbContexts.Where(c => c.ChangeTracker.HasChanges()))
        {
            try
            {
                total += await ctx.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                hasErrors = true;
                logger?.LogError(ex, "Failed to save changes in {DbContextType}", ctx.GetType().Name);
            }
        }

        if (hasErrors)
        {
            throw new InvalidOperationException("One or more database contexts failed to save changes. See inner logs for details.");
        }

        return total;
    }

    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        var result = await SaveChangesAsync(cancellationToken);
        return result > 0;
    }
}