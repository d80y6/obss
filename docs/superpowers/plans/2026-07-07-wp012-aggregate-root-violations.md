# WP-012: Fix Aggregate Root Violations Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fix 7 DDD aggregate root violations and 1 domain event location issue across 6 modules.

**Architecture:** Change base classes (Entity↔AggregateRoot) with zero behavioral impact for promotions; drop dedicated repository interfaces for demotions. `IRepository<T>` constraint stays `where T : class` so demoted entities still work with generic repo.

**Tech Stack:** .NET 9, EF Core, SharedKernel (Entity.cs, AggregateRoot.cs)

---

### Task 1: Promote Category to AggregateRoot (ProductCatalog)

**Files:**
- Modify: `src/Modules/ProductCatalog/Obss.ProductCatalog.Domain/Domain/Entities/Category.cs:7`

- [ ] **Change base class from Entity<Guid> to AggregateRoot<Guid>**

```csharp
public class Category : AggregateRoot<Guid>, ITenantEntity
```

- [ ] **Build and verify**

Run: `dotnet build src/Host/Obss.Host/Obss.Host.csproj --configuration Release --no-restore`
Expected: Build succeeded, 0 warnings, 0 errors

- [ ] **Commit**

```bash
git add src/Modules/ProductCatalog/Obss.ProductCatalog.Domain/Domain/Entities/Category.cs
git commit -m "fix: promote Category to AggregateRoot"
```

---

### Task 2: Promote WebhookEvent to AggregateRoot (EventManagement)

**Files:**
- Modify: `src/Modules/EventManagement/Obss.EventManagement.Domain/Entities/WebhookEvent.cs:5`

- [ ] **Change base class from Entity<Guid> to AggregateRoot<Guid>**

```csharp
public class WebhookEvent : AggregateRoot<Guid>
```

- [ ] **Build and verify**

Run: `dotnet build src/Host/Obss.Host/Obss.Host.csproj --configuration Release --no-restore`
Expected: Build succeeded, 0 warnings, 0 errors

- [ ] **Commit**

```bash
git add src/Modules/EventManagement/Obss.EventManagement.Domain/Entities/WebhookEvent.cs
git commit -m "fix: promote WebhookEvent to AggregateRoot"
```

---

### Task 3: Promote SubscriptionEntitlement to AggregateRoot (Subscriptions)

**Files:**
- Modify: `src/Modules/Subscriptions/Obss.Subscriptions.Domain/Entities/SubscriptionEntitlement.cs:7`

- [ ] **Change base class from Entity<Guid> to AggregateRoot<Guid>**

```csharp
public class SubscriptionEntitlement : AggregateRoot<Guid>
```

- [ ] **Build and verify**

Run: `dotnet build src/Host/Obss.Host/Obss.Host.csproj --configuration Release --no-restore`
Expected: Build succeeded, 0 warnings, 0 errors

- [ ] **Commit**

```bash
git add src/Modules/Subscriptions/Obss.Subscriptions.Domain/Entities/SubscriptionEntitlement.cs
git commit -m "fix: promote SubscriptionEntitlement to AggregateRoot"
```

---

### Task 4: Demote AuditPolicy to Entity (Audit)

**Files:**
- Modify: `src/Modules/Audit/Obss.Audit.Domain/Entities/AuditPolicy.cs:6`

- [ ] **Change base class from AggregateRoot<Guid> to Entity<Guid>**

```csharp
public sealed class AuditPolicy : Entity<Guid>, ITenantEntity
```

- [ ] **Build and verify**

Run: `dotnet build src/Host/Obss.Host/Obss.Host.csproj --configuration Release --no-restore`
Expected: Build succeeded, 0 warnings, 0 errors

- [ ] **Commit**

```bash
git add src/Modules/Audit/Obss.Audit.Domain/Entities/AuditPolicy.cs
git commit -m "fix: demote AuditPolicy to Entity"
```

---

### Task 5: Demote AuditAlertRule to Entity + remove repository (Audit)

**Files:**
- Modify: `src/Modules/Audit/Obss.Audit.Domain/Entities/AuditAlertRule.cs:7`
- Delete: `src/Modules/Audit/Obss.Audit.Application/Abstractions/IAuditAlertRuleRepository.cs`
- Delete: `src/Modules/Audit/Obss.Audit.Infrastructure/Persistence/Repositories/AuditAlertRuleRepository.cs`
- Modify: `src/Modules/Audit/Obss.Audit.Application/Commands/CreateAlertRule/CreateAlertRuleCommandHandler.cs`
- Modify: `src/Modules/Audit/Obss.Audit.Application/Queries/GetAlertRules/GetAlertRulesQueryHandler.cs`
- Modify: `src/Modules/Audit/Obss.Audit.Api/Endpoints/AuditEndpoints.cs:150`
- Modify: `src/Modules/Audit/Obss.Audit.Api/Extensions/AuditModuleRegistration.cs:21`

- [ ] **Change base class from AggregateRoot<Guid> to Entity<Guid>**

```csharp
public sealed class AuditAlertRule : Entity<Guid>, ITenantEntity
```

- [ ] **Delete `IAuditAlertRuleRepository` file**

Delete the entire file `src/Modules/Audit/Obss.Audit.Application/Abstractions/IAuditAlertRuleRepository.cs`.

- [ ] **Delete `AuditAlertRuleRepository` file**

Delete the entire file `src/Modules/Audit/Obss.Audit.Infrastructure/Persistence/Repositories/AuditAlertRuleRepository.cs`.

- [ ] **Update CreateAlertRuleCommandHandler to use IRepository<AuditAlertRule>**

Replace `IAuditAlertRuleRepository` with `IRepository<AuditAlertRule>`:

```csharp
using Obss.SharedKernel.Application.Abstractions;
// ... (remove using Obss.Audit.Application.Abstractions;)

public sealed class CreateAlertRuleCommandHandler : IRequestHandler<CreateAlertRuleCommand, Result<AuditAlertRuleDto>>
{
    private readonly IRepository<AuditAlertRule> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenant _currentTenant;

    public CreateAlertRuleCommandHandler(
        IRepository<AuditAlertRule> repository,
        IUnitOfWork unitOfWork,
        ICurrentTenant currentTenant)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
    }
    // ... rest unchanged
}
```

- [ ] **Update GetAlertRulesQueryHandler to use IRepository<AuditAlertRule>**

Replace `IAuditAlertRuleRepository` with `IRepository<AuditAlertRule>` and inline the `GetAllAsync` call:

```csharp
using Obss.SharedKernel.Application.Abstractions;
// (remove using Obss.Audit.Application.Abstractions;)

public sealed class GetAlertRulesQueryHandler : IRequestHandler<GetAlertRulesQuery, Result<IReadOnlyList<AuditAlertRuleDto>>>
{
    private readonly IRepository<AuditAlertRule> _repository;

    public GetAlertRulesQueryHandler(IRepository<AuditAlertRule> repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<AuditAlertRuleDto>>> Handle(GetAlertRulesQuery request, CancellationToken cancellationToken)
    {
        var rules = await _repository.GetAllAsync(cancellationToken);
        var result = rules.Adapt<List<AuditAlertRuleDto>>();
        return Result.Success<IReadOnlyList<AuditAlertRuleDto>>(result);
    }
}
```

- [ ] **Update AuditEndpoints.cs to use IRepository<AuditAlertRule>**

Replace `IAuditAlertRuleRepository repository` with `IRepository<AuditAlertRule> repository`:

```csharp
group.MapGet("/alert-rules/{id:guid}", async (Guid id, IRepository<AuditAlertRule> repository) =>
{
    var rule = await repository.GetByIdAsync(id);
    return rule is not null
        ? (IResult)TypedResults.Ok(rule.Adapt<AuditAlertRuleDto>())
        : (IResult)TypedResults.NotFound();
});
```

Add the required using:
```csharp
using Obss.SharedKernel.Application.Abstractions;
```

- [ ] **Remove DI registration for AuditAlertRuleRepository**

In `AuditModuleRegistration.cs`, remove line:
```csharp
services.AddScoped<IAuditAlertRuleRepository, AuditAlertRuleRepository>();
```
(Line 21)

- [ ] **Build and verify**

Run: `dotnet build src/Host/Obss.Host/Obss.Host.csproj --configuration Release --no-restore`
Expected: Build succeeded, 0 warnings, 0 errors

- [ ] **Commit**

```bash
git add src/Modules/Audit/
git commit -m "fix: demote AuditAlertRule to Entity, remove dedicated repository"
```

---

### Task 6: Demote TopologyMap to Entity + remove repository (NetworkInventory)

**Files:**
- Modify: `src/Modules/NetworkInventory/Obss.NetworkInventory.Domain/Entities/TopologyMap.cs:5`
- Delete: `src/Modules/NetworkInventory/Obss.NetworkInventory.Infrastructure/Persistence/Repositories/TopologyMapRepository.cs`

- [ ] **Change base class from AggregateRoot<Guid> to Entity<Guid>**

```csharp
public class TopologyMap : Entity<Guid>
```

- [ ] **Delete `TopologyMapRepository`**

Delete the entire file `src/Modules/NetworkInventory/Obss.NetworkInventory.Infrastructure/Persistence/Repositories/TopologyMapRepository.cs`. The handlers already use `IRepository<TopologyMap>` (generic), not the concrete repository, so no handler changes needed.

- [ ] **Build and verify**

Run: `dotnet build src/Host/Obss.Host/Obss.Host.csproj --configuration Release --no-restore`
Expected: Build succeeded, 0 warnings, 0 errors

- [ ] **Commit**

```bash
git add src/Modules/NetworkInventory/
git commit -m "fix: demote TopologyMap to Entity, remove unused repository"
```

---

### Task 7: Remove WorkflowMetric repository (Workflow)

WorkflowMetric is already `Entity<Guid>` — the violation is that it has its own dedicated repository interface despite not being an aggregate root.

**Files:**
- Delete: `src/Modules/Workflow/Obss.Workflow.Application/Abstractions/IWorkflowMetricRepository.cs`
- Delete: `src/Modules/Workflow/Obss.Workflow.Infrastructure/Persistence/Repositories/WorkflowMetricRepository.cs`
- Modify: `src/Modules/Workflow/Obss.Workflow.Application/Queries/GetWorkflowMetrics/GetWorkflowMetricsQueryHandler.cs`
- Modify: `src/Modules/Workflow/Obss.Workflow.Api/Extensions/WorkflowModuleRegistration.cs:22`

- [ ] **Delete `IWorkflowMetricRepository` file**

Delete `src/Modules/Workflow/Obss.Workflow.Application/Abstractions/IWorkflowMetricRepository.cs`.

- [ ] **Delete `WorkflowMetricRepository` file**

Delete `src/Modules/Workflow/Obss.Workflow.Infrastructure/Persistence/Repositories/WorkflowMetricRepository.cs`.

- [ ] **Update GetWorkflowMetricsQueryHandler to use IRepository<WorkflowMetric> + inline query**

Replace `IWorkflowMetricRepository` with `IRepository<WorkflowMetric>` and inline the `GetMetricsAsync` query logic:

```csharp
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Infrastructure.Persistence;
using Obss.Workflow.Application.DTOs;
using Obss.Workflow.Domain.Entities;
using Obss.Workflow.Domain.ValueObjects;

namespace Obss.Workflow.Application.Queries.GetWorkflowMetrics;

public sealed class GetWorkflowMetricsQueryHandler : IRequestHandler<GetWorkflowMetricsQuery, Result<IReadOnlyList<WorkflowMetricDto>>>
{
    private readonly IRepository<WorkflowMetric> _repository;

    public GetWorkflowMetricsQueryHandler(IRepository<WorkflowMetric> repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<WorkflowMetricDto>>> Handle(GetWorkflowMetricsQuery request, CancellationToken cancellationToken)
    {
        MetricType? metricType = null;
        if (!string.IsNullOrWhiteSpace(request.MetricType) &&
            Enum.TryParse<MetricType>(request.MetricType, true, out var parsed))
        {
            metricType = parsed;
        }

        // Inline query from former WorkflowMetricRepository.GetMetricsAsync
        var query = _repository.GetQueryable();

        if (request.WorkflowDefinitionId.HasValue)
            query = query.Where(m => m.WorkflowDefinitionId == request.WorkflowDefinitionId.Value);

        if (metricType.HasValue)
            query = query.Where(m => m.MetricType == metricType.Value);

        if (request.From.HasValue)
            query = query.Where(m => m.TimeBucket >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(m => m.TimeBucket <= request.To.Value);

        query = query.OrderByDescending(m => m.TimeBucket);

        var metrics = await query.ToListAsync(cancellationToken);
        var result = metrics.Adapt<List<WorkflowMetricDto>>();
        return Result.Success<IReadOnlyList<WorkflowMetricDto>>(result);
    }
}
```

- [ ] **Remove DI registration for WorkflowMetricRepository**

In `WorkflowModuleRegistration.cs`, remove line:
```csharp
services.AddScoped<IWorkflowMetricRepository, WorkflowMetricRepository>();
```
(Line 22)

- [ ] **Add GetQueryable to IRepository<T> and EfRepository<T>**

`IRepository<T>` does not expose `IQueryable<T>`. The handler needs it for the filtered/ordered query. Add to `IRepository.cs`:

```csharp
IQueryable<T> GetQueryable();
```

Add to `EfRepository.cs`:

```csharp
using System.Linq; // already present via System.Linq.Expressions

public virtual IQueryable<T> GetQueryable()
{
    return DbSet.AsQueryable();
}
```

- [ ] **Update handler to use IRepository<WorkflowMetric> with inline query**

```csharp
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.Workflow.Application.DTOs;
using Obss.Workflow.Domain.Entities;
using Obss.Workflow.Domain.ValueObjects;

namespace Obss.Workflow.Application.Queries.GetWorkflowMetrics;

public sealed class GetWorkflowMetricsQueryHandler : IRequestHandler<GetWorkflowMetricsQuery, Result<IReadOnlyList<WorkflowMetricDto>>>
{
    private readonly IRepository<WorkflowMetric> _repository;

    public GetWorkflowMetricsQueryHandler(IRepository<WorkflowMetric> repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<WorkflowMetricDto>>> Handle(GetWorkflowMetricsQuery request, CancellationToken cancellationToken)
    {
        MetricType? metricType = null;
        if (!string.IsNullOrWhiteSpace(request.MetricType) &&
            Enum.TryParse<MetricType>(request.MetricType, true, out var parsed))
        {
            metricType = parsed;
        }

        var query = _repository.GetQueryable();

        if (request.WorkflowDefinitionId.HasValue)
            query = query.Where(m => m.WorkflowDefinitionId == request.WorkflowDefinitionId.Value);

        if (metricType.HasValue)
            query = query.Where(m => m.MetricType == metricType.Value);

        if (request.From.HasValue)
            query = query.Where(m => m.TimeBucket >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(m => m.TimeBucket <= request.To.Value);

        query = query.OrderByDescending(m => m.TimeBucket);

        var metrics = await query.ToListAsync(cancellationToken);
        var result = metrics.Adapt<List<WorkflowMetricDto>>();
        return Result.Success<IReadOnlyList<WorkflowMetricDto>>(result);
    }
}
```

- [ ] **Build and verify**

Run: `dotnet build src/Host/Obss.Host/Obss.Host.csproj --configuration Release --no-restore`
Expected: Build succeeded, 0 warnings, 0 errors

- [ ] **Commit**

```bash
git add src/Modules/Workflow/ src/Shared/Obss.SharedKernel/
git commit -m "fix: remove WorkflowMetric repository, inline query in handler"
```

---

### Task 8: Extract InvoiceCancelledDomainEvent to Events folder (Invoices)

**Files:**
- Create: `src/Modules/Invoices/Obss.Invoices.Domain/Domain/Events/InvoiceCancelledDomainEvent.cs`
- Modify: `src/Modules/Invoices/Obss.Invoices.Domain/Domain/Entities/Invoice.cs` (remove inline event)

- [ ] **Create InvoiceCancelledDomainEvent.cs**

```csharp
using MediatR;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Invoices.Domain.Events;

public sealed class InvoiceCancelledDomainEvent : DomainEvent, INotification
{
    public InvoiceCancelledDomainEvent(Guid invoiceId, string invoiceNumber, string reason)
    {
        InvoiceId = invoiceId;
        InvoiceNumber = invoiceNumber;
        Reason = reason;
    }

    public Guid InvoiceId { get; }
    public string InvoiceNumber { get; }
    public string Reason { get; }
}
```

- [ ] **Remove inline event from Invoice.cs**

Delete lines 278-290 from `Invoice.cs` (the `InvoiceCancelledDomainEvent` class definition at the end of the file).

- [ ] **Build and verify**

Run: `dotnet build src/Host/Obss.Host/Obss.Host.csproj --configuration Release --no-restore`
Expected: Build succeeded, 0 warnings, 0 errors

- [ ] **Commit**

```bash
git add src/Modules/Invoices/
git commit -m "fix: extract InvoiceCancelledDomainEvent to Events folder"
```

---

### Task 9: Full solution build + SharedKernel tests

- [ ] **Run full solution build**

Run: `dotnet build Obss.sln --configuration Release --no-restore`
Expected: Build succeeded, 0 warnings, 0 errors

- [ ] **Run SharedKernel tests**

Run: `dotnet test tests/Obss.SharedKernel.Tests/ --no-build --configuration Release`
Expected: Passed! - Failed: 0, Passed: 74

- [ ] **Commit final state**

```bash
git commit -m "chore: final verification after aggregate root fixes"
```
