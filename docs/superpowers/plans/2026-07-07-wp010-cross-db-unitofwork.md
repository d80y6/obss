# WP-010: Fix Cross-DB UnitOfWork

**Goal:** Eliminate the cross-module atomicity risk by making `UnitOfWork` enforce single-context saves per handler.

**Architecture:** The current `UnitOfWork` iterates ALL 22 DbContexts and saves any with pending changes, but without distributed transaction safety. The fix: `UnitOfWork` will detect when multiple contexts have pending changes, throw to prevent unsafe cross-module saves, and save only the single module's context for normal operations.

**Tech Stack:** .NET 9, EF Core, SharedKernel

---

### Task 1: Modify UnitOfWork to enforce single-context saves

**Files:**
- Modify: `src/Shared/Obss.SharedKernel/Infrastructure/Persistence/UnitOfWork.cs`

Change `UnitOfWork` from saving ALL contexts with changes to throwing when multiple contexts have pending changes, ensuring each handler saves only its own module's data.

```csharp
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
                "Each command handler must save only its own module's context. " +
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
```

### Task 2: Build and verify

- [ ] **Run build:** `dotnet build src/Host/Obss.Host/Obss.Host.csproj --configuration Release --no-restore`
  Expected: Build succeeded, 0 warnings, 0 errors

- [ ] **Run tests:** `dotnet test tests/Obss.SharedKernel.Tests/ --no-build --configuration Release`
  Expected: Passed! - Failed: 0, Passed: 74

- [ ] **Full solution build:** `dotnet build Obss.sln --configuration Release --no-restore`
  Expected: Build succeeded, 0 warnings, 0 errors
