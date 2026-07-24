# AAA Frontend Pages Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add AAA management frontend pages (Dashboard, NAS CRUD, Session monitoring, Audit Logs) backed by 5 new API endpoints and an AaaAuditLog entity.

**Architecture:** Backend additions follow CQRS/MediatR patterns (new commands/queries/handlers + domain event handlers). Frontend follows existing Next.js App Router patterns with React Query hooks, DataTable, FormPageLayout components.

**Tech Stack:** .NET 9 + MediatR + EF Core (backend), Next.js 16 + React 19 + TanStack Query + Tailwind CSS 4 + Radix UI (frontend)

---

### Task 1: PaginatedResult Contract + DTOs

**Files:**
- Create: `src/Modules/AAA/Obss.AAA.Application/Contracts/PaginatedResult.cs`
- Create: `src/Modules/AAA/Obss.AAA.Application/DTOs/AaaMetricsDto.cs`
- Create: `src/Modules/AAA/Obss.AAA.Application/DTOs/AaaAuditLogDto.cs`

- [ ] **Create PaginatedResult.cs**

```csharp
namespace Obss.AAA.Application.Contracts;

public sealed record PaginatedResult<T>(IReadOnlyList<T> Items, int TotalCount);
```

- [ ] **Create AaaMetricsDto.cs**

```csharp
namespace Obss.AAA.Application.DTOs;

public sealed record AaaMetricsDto(
    int TotalNas,
    int ActiveNas,
    int InactiveNas,
    int ActiveSessions,
    int SessionsToday,
    long TotalInputOctets,
    long TotalOutputOctets);
```

- [ ] **Create AaaAuditLogDto.cs**

```csharp
namespace Obss.AAA.Application.DTOs;

public sealed record AaaAuditLogDto(
    Guid Id,
    string TenantId,
    string EventType,
    string? Username,
    Guid? NasId,
    string? NasIpAddress,
    string? Detail,
    DateTime Timestamp);
```

---

### Task 2: AaaEventType Enum + AaaAuditLog Entity + EF Config

**Files:**
- Create: `src/Modules/AAA/Obss.AAA.Domain/ValueObjects/AaaEventType.cs`
- Create: `src/Modules/AAA/Obss.AAA.Domain/Entities/AaaAuditLog.cs`
- Create: `src/Modules/AAA/Obss.AAA.Infrastructure/Persistence/Configurations/AaaAuditLogConfiguration.cs`

- [ ] **Create AaaEventType.cs**

```csharp
namespace Obss.AAA.Domain.ValueObjects;

public enum AaaEventType
{
    AuthenticationSuccess = 1,
    AuthenticationFailure = 2,
    AccountingStart = 3,
    AccountingStop = 4,
    AccountingInterim = 5,
    NasRegistered = 6,
    NasUpdated = 7,
    NasDeleted = 8,
    NasStatusChanged = 9
}
```

- [ ] **Create AaaAuditLog.cs**

```csharp
using Obss.SharedKernel.Domain.Common;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.AAA.Domain.Entities;

public class AaaAuditLog : AggregateRoot<Guid>, ITenantEntity
{
    private AaaAuditLog() { }

    private AaaAuditLog(
        Guid id,
        string tenantId,
        string eventType,
        string? username,
        Guid? nasId,
        string? nasIpAddress,
        string? detail)
        : base(id)
    {
        TenantId = tenantId;
        EventType = eventType;
        Username = username;
        NasId = nasId;
        NasIpAddress = nasIpAddress;
        Detail = detail;
        Timestamp = DateTime.UtcNow;
    }

    public string TenantId { get; private set; } = string.Empty;
    public string EventType { get; private set; } = string.Empty;
    public string? Username { get; private set; }
    public Guid? NasId { get; private set; }
    public string? NasIpAddress { get; private set; }
    public string? Detail { get; private set; }
    public DateTime Timestamp { get; private set; }

    public static AaaAuditLog Create(
        string tenantId,
        string eventType,
        string? username = null,
        Guid? nasId = null,
        string? nasIpAddress = null,
        string? detail = null)
    {
        return new AaaAuditLog(Guid.NewGuid(), tenantId, eventType, username, nasId, nasIpAddress, detail);
    }
}
```

- [ ] **Create AaaAuditLogConfiguration.cs**

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.AAA.Domain.Entities;

namespace Obss.AAA.Infrastructure.Persistence.Configurations;

public sealed class AaaAuditLogConfiguration : IEntityTypeConfiguration<AaaAuditLog>
{
    public void Configure(EntityTypeBuilder<AaaAuditLog> builder)
    {
        builder.ToTable("aaa_audit_logs");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).ValueGeneratedNever();

        builder.Property(l => l.TenantId).HasColumnName("tenant_id").HasMaxLength(100).IsRequired();
        builder.Property(l => l.EventType).HasColumnName("event_type").HasMaxLength(50).IsRequired();
        builder.Property(l => l.Username).HasColumnName("username").HasMaxLength(200);
        builder.Property(l => l.NasId).HasColumnName("nas_id");
        builder.Property(l => l.NasIpAddress).HasColumnName("nas_ip_address").HasMaxLength(45);
        builder.Property(l => l.Detail).HasColumnName("detail").HasColumnType("jsonb");
        builder.Property(l => l.Timestamp).HasColumnName("timestamp").IsRequired();

        builder.HasIndex(l => l.Timestamp).HasDatabaseName("ix_aaa_audit_logs_timestamp");
        builder.HasIndex(l => l.EventType).HasDatabaseName("ix_aaa_audit_logs_event_type");
        builder.HasIndex(l => l.NasId).HasDatabaseName("ix_aaa_audit_logs_nas_id");
        builder.HasIndex(l => l.Username).HasDatabaseName("ix_aaa_audit_logs_username");
    }
}
```

- [ ] **Add DbSet to AaaDbContext.cs**

Edit `/home/ubuntu/obss/src/Modules/AAA/Obss.AAA.Infrastructure/Persistence/AaaDbContext.cs`:

After `public DbSet<RadiusSession> RadiusSessions => Set<RadiusSession>();`
Add:
```csharp
    public DbSet<AaaAuditLog> AaaAuditLogs => Set<AaaAuditLog>();
```

---

### Task 3: IAaaAuditLogRepository + AaaAuditLogRepository

**Files:**
- Create: `src/Modules/AAA/Obss.AAA.Application/Abstractions/IAaaAuditLogRepository.cs`
- Create: `src/Modules/AAA/Obss.AAA.Infrastructure/Persistence/Repositories/AaaAuditLogRepository.cs`

- [ ] **Create IAaaAuditLogRepository.cs**

```csharp
using Obss.AAA.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.AAA.Application.Abstractions;

public interface IAaaAuditLogRepository : IRepository<AaaAuditLog>
{
    Task<(IReadOnlyList<AaaAuditLog> Items, int TotalCount)> GetPaginatedAsync(
        int offset,
        int limit,
        string? eventType = null,
        string? username = null,
        Guid? nasId = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        CancellationToken cancellationToken = default);

    Task<int> CountByDateRangeAsync(
        DateTime dateFrom,
        DateTime dateTo,
        CancellationToken cancellationToken = default);
}
```

- [ ] **Create AaaAuditLogRepository.cs**

```csharp
using Microsoft.EntityFrameworkCore;
using Obss.AAA.Application.Abstractions;
using Obss.AAA.Domain.Entities;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.AAA.Infrastructure.Persistence.Repositories;

public sealed class AaaAuditLogRepository : EfRepository<AaaAuditLog>, IAaaAuditLogRepository
{
    public AaaAuditLogRepository(AaaDbContext context) : base(context)
    {
    }

    public async Task<(IReadOnlyList<AaaAuditLog> Items, int TotalCount)> GetPaginatedAsync(
        int offset,
        int limit,
        string? eventType = null,
        string? username = null,
        Guid? nasId = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();

        if (!string.IsNullOrEmpty(eventType))
            query = query.Where(l => l.EventType == eventType);
        if (!string.IsNullOrEmpty(username))
            query = query.Where(l => l.Username != null && l.Username == username);
        if (nasId.HasValue)
            query = query.Where(l => l.NasId == nasId.Value);
        if (dateFrom.HasValue)
            query = query.Where(l => l.Timestamp >= dateFrom.Value);
        if (dateTo.HasValue)
            query = query.Where(l => l.Timestamp <= dateTo.Value);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(l => l.Timestamp)
            .Skip(offset)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<int> CountByDateRangeAsync(
        DateTime dateFrom,
        DateTime dateTo,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.CountAsync(
            l => l.Timestamp >= dateFrom && l.Timestamp <= dateTo,
            cancellationToken);
    }
}
```

---

### Task 4: UpdateNas Command/Handler/Validator

**Files:**
- Create: `src/Modules/AAA/Obss.AAA.Application/Commands/UpdateNas/UpdateNasCommand.cs`
- Create: `src/Modules/AAA/Obss.AAA.Application/Commands/UpdateNas/UpdateNasCommandHandler.cs`
- Create: `src/Modules/AAA/Obss.AAA.Application/Commands/UpdateNas/UpdateNasCommandValidator.cs`

- [ ] **Create UpdateNasCommand.cs**

```csharp
using MediatR;
using Obss.AAA.Application.DTOs;
using Obss.AAA.Domain.ValueObjects;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.AAA.Application.Commands.UpdateNas;

public sealed record UpdateNasCommand(
    Guid Id,
    string Name,
    string NasIpAddress,
    string NasSecret,
    NasType NasType,
    string? Location) : IRequest<Result<NasDto>>;
```

- [ ] **Create UpdateNasCommandHandler.cs**

```csharp
using Mapster;
using MediatR;
using Obss.AAA.Application.Abstractions;
using Obss.AAA.Application.DTOs;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.AAA.Application.Commands.UpdateNas;

public sealed class UpdateNasCommandHandler : IRequestHandler<UpdateNasCommand, Result<NasDto>>
{
    private readonly INasRepository _nasRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateNasCommandHandler(
        INasRepository nasRepository,
        IUnitOfWork unitOfWork)
    {
        _nasRepository = nasRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<NasDto>> Handle(UpdateNasCommand request, CancellationToken cancellationToken)
    {
        var nas = await _nasRepository.GetByIdAsync(request.Id, cancellationToken);

        if (nas is null)
            return Result.Failure<NasDto>(Error.NotFound("NetworkAccessServer", request.Id));

        nas.UpdateSettings(
            request.Name,
            request.NasIpAddress,
            request.NasSecret,
            request.NasType,
            request.Location);

        await _nasRepository.UpdateAsync(nas, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(nas.Adapt<NasDto>());
    }
}
```

- [ ] **Create UpdateNasCommandValidator.cs**

```csharp
using FluentValidation;

namespace Obss.AAA.Application.Commands.UpdateNas;

public sealed class UpdateNasCommandValidator : AbstractValidator<UpdateNasCommand>
{
    public UpdateNasCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NasIpAddress).NotEmpty().MaximumLength(45);
        RuleFor(x => x.NasSecret).NotEmpty().MinimumLength(8).MaximumLength(500);
        RuleFor(x => x.NasType).IsInEnum();
        RuleFor(x => x.Location).MaximumLength(500);
    }
}
```

---

### Task 5: DeleteNas Command/Handler

**Files:**
- Create: `src/Modules/AAA/Obss.AAA.Application/Commands/DeleteNas/DeleteNasCommand.cs`
- Create: `src/Modules/AAA/Obss.AAA.Application/Commands/DeleteNas/DeleteNasCommandHandler.cs`

- [ ] **Create DeleteNasCommand.cs**

```csharp
using MediatR;
using Obss.AAA.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.AAA.Application.Commands.DeleteNas;

public sealed record DeleteNasCommand(Guid Id) : IRequest<Result<NasDto>>;
```

- [ ] **Create DeleteNasCommandHandler.cs**

```csharp
using Mapster;
using MediatR;
using Obss.AAA.Application.Abstractions;
using Obss.AAA.Application.DTOs;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.AAA.Application.Commands.DeleteNas;

public sealed class DeleteNasCommandHandler : IRequestHandler<DeleteNasCommand, Result<NasDto>>
{
    private readonly INasRepository _nasRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteNasCommandHandler(
        INasRepository nasRepository,
        IUnitOfWork unitOfWork)
    {
        _nasRepository = nasRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<NasDto>> Handle(DeleteNasCommand request, CancellationToken cancellationToken)
    {
        var nas = await _nasRepository.GetByIdAsync(request.Id, cancellationToken);

        if (nas is null)
            return Result.Failure<NasDto>(Error.NotFound("NetworkAccessServer", request.Id));

        await _nasRepository.DeleteAsync(nas, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(nas.Adapt<NasDto>());
    }
}
```

---

### Task 6: Update GetAllNasDevices with Pagination

**Files:**
- Modify: `src/Modules/AAA/Obss.AAA.Application/Queries/GetAllNasDevices/GetAllNasDevicesQuery.cs`
- Modify: `src/Modules/AAA/Obss.AAA.Application/Queries/GetAllNasDevices/GetAllNasDevicesQueryHandler.cs`

- [ ] **Update GetAllNasDevicesQuery.cs**

```csharp
using MediatR;
using Obss.AAA.Application.Contracts;
using Obss.AAA.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.AAA.Application.Queries.GetAllNasDevices;

public sealed record GetAllNasDevicesQuery(
    int Page = 1,
    int PageSize = 20,
    string? NasType = null) : IRequest<Result<PaginatedResult<NasDto>>>;
```

- [ ] **Update GetAllNasDevicesQueryHandler.cs**

```csharp
using Mapster;
using MediatR;
using Obss.AAA.Application.Abstractions;
using Obss.AAA.Application.Contracts;
using Obss.AAA.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.AAA.Application.Queries.GetAllNasDevices;

public sealed class GetAllNasDevicesQueryHandler : IRequestHandler<GetAllNasDevicesQuery, Result<PaginatedResult<NasDto>>>
{
    private readonly INasRepository _nasRepository;

    public GetAllNasDevicesQueryHandler(INasRepository nasRepository)
    {
        _nasRepository = nasRepository;
    }

    public async Task<Result<PaginatedResult<NasDto>>> Handle(GetAllNasDevicesQuery request, CancellationToken cancellationToken)
    {
        var allNas = await _nasRepository.GetAllAsync(cancellationToken);

        var filtered = string.IsNullOrEmpty(request.NasType)
            ? allNas
            : allNas.Where(n => n.NasType.ToString() == request.NasType).ToList();

        var total = filtered.Count;
        var paged = filtered
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return Result.Success(new PaginatedResult<NasDto>(
            paged.Adapt<IReadOnlyList<NasDto>>(),
            total));
    }
}
```

---

### Task 7: GetAaaMetrics Query/Handler

**Files:**
- Create: `src/Modules/AAA/Obss.AAA.Application/Queries/GetAaaMetrics/GetAaaMetricsQuery.cs`
- Create: `src/Modules/AAA/Obss.AAA.Application/Queries/GetAaaMetrics/GetAaaMetricsQueryHandler.cs`

- [ ] **Create GetAaaMetricsQuery.cs**

```csharp
using MediatR;
using Obss.AAA.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.AAA.Application.Queries.GetAaaMetrics;

public sealed record GetAaaMetricsQuery : IRequest<Result<AaaMetricsDto>>;
```

- [ ] **Create GetAaaMetricsQueryHandler.cs**

```csharp
using MediatR;
using Obss.AAA.Application.Abstractions;
using Obss.AAA.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.AAA.Application.Queries.GetAaaMetrics;

public sealed class GetAaaMetricsQueryHandler : IRequestHandler<GetAaaMetricsQuery, Result<AaaMetricsDto>>
{
    private readonly INasRepository _nasRepository;
    private readonly IRadiusSessionRepository _sessionRepository;

    public GetAaaMetricsQueryHandler(
        INasRepository nasRepository,
        IRadiusSessionRepository sessionRepository)
    {
        _nasRepository = nasRepository;
        _sessionRepository = sessionRepository;
    }

    public async Task<Result<AaaMetricsDto>> Handle(GetAaaMetricsQuery request, CancellationToken cancellationToken)
    {
        var allNas = await _nasRepository.GetAllAsync(cancellationToken);
        var activeNas = allNas.Count(n => n.Status == "Active");
        var inactiveNas = allNas.Count(n => n.Status == "Inactive");
        var activeSessions = await _sessionRepository.CountActiveSessionsAsync(cancellationToken);

        var allSessions = await _sessionRepository.GetAllAsync(cancellationToken);
        var today = DateTime.UtcNow.Date;
        var todaySessions = allSessions.Count(s => s.StartedAt >= today);
        var totalInput = allSessions.Sum(s => s.InputOctets);
        var totalOutput = allSessions.Sum(s => s.OutputOctets);

        return Result.Success(new AaaMetricsDto(
            allNas.Count,
            activeNas,
            inactiveNas,
            activeSessions,
            todaySessions,
            totalInput,
            totalOutput));
    }
}
```

---

### Task 8: GetSessions Paginated Query/Handler

**Files:**
- Create: `src/Modules/AAA/Obss.AAA.Application/Queries/GetSessions/GetSessionsQuery.cs`
- Create: `src/Modules/AAA/Obss.AAA.Application/Queries/GetSessions/GetSessionsQueryHandler.cs`

- [ ] **Create GetSessionsQuery.cs**

```csharp
using MediatR;
using Obss.AAA.Application.Contracts;
using Obss.AAA.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.AAA.Application.Queries.GetSessions;

public sealed record GetSessionsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Status = null,
    Guid? NasId = null,
    string? Username = null,
    DateTime? DateFrom = null,
    DateTime? DateTo = null) : IRequest<Result<PaginatedResult<RadiusSessionDto>>>;
```

- [ ] **Create GetSessionsQueryHandler.cs**

```csharp
using Mapster;
using MediatR;
using Obss.AAA.Application.Abstractions;
using Obss.AAA.Application.Contracts;
using Obss.AAA.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.AAA.Application.Queries.GetSessions;

public sealed class GetSessionsQueryHandler : IRequestHandler<GetSessionsQuery, Result<PaginatedResult<RadiusSessionDto>>>
{
    private readonly IRadiusSessionRepository _sessionRepository;

    public GetSessionsQueryHandler(IRadiusSessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }

    public async Task<Result<PaginatedResult<RadiusSessionDto>>> Handle(GetSessionsQuery request, CancellationToken cancellationToken)
    {
        var allSessions = await _sessionRepository.GetAllAsync(cancellationToken);

        var filtered = allSessions.AsEnumerable();

        if (!string.IsNullOrEmpty(request.Status))
            filtered = filtered.Where(s => s.SessionStatus.ToString() == request.Status);
        if (request.NasId.HasValue)
            filtered = filtered.Where(s => s.NasId == request.NasId.Value);
        if (!string.IsNullOrEmpty(request.Username))
            filtered = filtered.Where(s => s.Username.Contains(request.Username, StringComparison.OrdinalIgnoreCase));
        if (request.DateFrom.HasValue)
            filtered = filtered.Where(s => s.StartedAt >= request.DateFrom.Value);
        if (request.DateTo.HasValue)
            filtered = filtered.Where(s => s.StartedAt <= request.DateTo.Value);

        var list = filtered.OrderByDescending(s => s.StartedAt).ToList();
        var total = list.Count;
        var paged = list.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList();

        return Result.Success(new PaginatedResult<RadiusSessionDto>(
            paged.Adapt<IReadOnlyList<RadiusSessionDto>>(),
            total));
    }
}
```

---

### Task 9: GetAaaLogs Query/Handler

**Files:**
- Create: `src/Modules/AAA/Obss.AAA.Application/Queries/GetAaaLogs/GetAaaLogsQuery.cs`
- Create: `src/Modules/AAA/Obss.AAA.Application/Queries/GetAaaLogs/GetAaaLogsQueryHandler.cs`

- [ ] **Create GetAaaLogsQuery.cs**

```csharp
using MediatR;
using Obss.AAA.Application.Contracts;
using Obss.AAA.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.AAA.Application.Queries.GetAaaLogs;

public sealed record GetAaaLogsQuery(
    int Page = 1,
    int PageSize = 20,
    string? EventType = null,
    string? Username = null,
    Guid? NasId = null,
    DateTime? DateFrom = null,
    DateTime? DateTo = null) : IRequest<Result<PaginatedResult<AaaAuditLogDto>>>;
```

- [ ] **Create GetAaaLogsQueryHandler.cs**

```csharp
using Mapster;
using MediatR;
using Obss.AAA.Application.Abstractions;
using Obss.AAA.Application.Contracts;
using Obss.AAA.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.AAA.Application.Queries.GetAaaLogs;

public sealed class GetAaaLogsQueryHandler : IRequestHandler<GetAaaLogsQuery, Result<PaginatedResult<AaaAuditLogDto>>>
{
    private readonly IAaaAuditLogRepository _logRepository;

    public GetAaaLogsQueryHandler(IAaaAuditLogRepository logRepository)
    {
        _logRepository = logRepository;
    }

    public async Task<Result<PaginatedResult<AaaAuditLogDto>>> Handle(GetAaaLogsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _logRepository.GetPaginatedAsync(
            (request.Page - 1) * request.PageSize,
            request.PageSize,
            request.EventType,
            request.Username,
            request.NasId,
            request.DateFrom,
            request.DateTo,
            cancellationToken);

        return Result.Success(new PaginatedResult<AaaAuditLogDto>(
            items.Adapt<IReadOnlyList<AaaAuditLogDto>>(),
            totalCount));
    }
}
```

---

### Task 10: Domain Event Handlers for Audit Logging

**Files:**
- Create: `src/Modules/AAA/Obss.AAA.Infrastructure/EventHandlers/LogSessionStartedHandler.cs`
- Create: `src/Modules/AAA/Obss.AAA.Infrastructure/EventHandlers/LogSessionStoppedHandler.cs`

- [ ] **Create LogSessionStartedHandler.cs**

```csharp
using MediatR;
using Obss.AAA.Application.Abstractions;
using Obss.AAA.Domain.Entities;
using Obss.AAA.Domain.Events;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.AAA.Infrastructure.EventHandlers;

public sealed class LogSessionStartedHandler : INotificationHandler<RadiusSessionStartedDomainEvent>
{
    private readonly IAaaAuditLogRepository _logRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenant _currentTenant;

    public LogSessionStartedHandler(
        IAaaAuditLogRepository logRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenant currentTenant)
    {
        _logRepository = logRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
    }

    public async Task Handle(RadiusSessionStartedDomainEvent notification, CancellationToken cancellationToken)
    {
        var log = AaaAuditLog.Create(
            _currentTenant.TenantId ?? string.Empty,
            "AccountingStart",
            notification.Username,
            notification.NasId,
            detail: $"{{\"sessionId\":\"{notification.SessionId}\",\"radiusSessionId\":\"{notification.RadiusSessionId}\"}}");

        await _logRepository.AddAsync(log, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
```

- [ ] **Create LogSessionStoppedHandler.cs**

```csharp
using MediatR;
using Obss.AAA.Application.Abstractions;
using Obss.AAA.Domain.Entities;
using Obss.AAA.Domain.Events;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.AAA.Infrastructure.EventHandlers;

public sealed class LogSessionStoppedHandler : INotificationHandler<RadiusSessionStoppedDomainEvent>
{
    private readonly IAaaAuditLogRepository _logRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenant _currentTenant;

    public LogSessionStoppedHandler(
        IAaaAuditLogRepository logRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenant currentTenant)
    {
        _logRepository = logRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
    }

    public async Task Handle(RadiusSessionStoppedDomainEvent notification, CancellationToken cancellationToken)
    {
        var log = AaaAuditLog.Create(
            _currentTenant.TenantId ?? string.Empty,
            "AccountingStop",
            notification.Username,
            notification.NasId,
            detail: $"{{\"sessionId\":\"{notification.SessionId}\",\"acctSessionTime\":{notification.AcctSessionTime},\"inputOctets\":{notification.InputOctets},\"outputOctets\":{notification.OutputOctets}}}");

        await _logRepository.AddAsync(log, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
```

---

### Task 11: Audit Logging in Existing Command Handlers

**Files:**
- Modify: `src/Modules/AAA/Obss.AAA.Application/Commands/RegisterNas/RegisterNasCommandHandler.cs`
- Modify: `src/Modules/AAA/Obss.AAA.Application/Commands/UpdateNasStatus/UpdateNasStatusCommandHandler.cs`

- [ ] **Modify RegisterNasCommandHandler.cs** to inject `IAaaAuditLogRepository` and `ICurrentTenant`, then log after save:

```csharp
using Mapster;
using MediatR;
using Obss.AAA.Application.Abstractions;
using Obss.AAA.Application.DTOs;
using Obss.AAA.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.AAA.Application.Commands.RegisterNas;

public sealed class RegisterNasCommandHandler : IRequestHandler<RegisterNasCommand, Result<NasDto>>
{
    private readonly INasRepository _nasRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenant _currentTenant;
    private readonly IAaaAuditLogRepository _logRepository;

    public RegisterNasCommandHandler(
        INasRepository nasRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenant currentTenant,
        IAaaAuditLogRepository logRepository)
    {
        _nasRepository = nasRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
        _logRepository = logRepository;
    }

    public async Task<Result<NasDto>> Handle(RegisterNasCommand request, CancellationToken cancellationToken)
    {
        var nas = NetworkAccessServer.Create(
            _currentTenant.TenantId ?? string.Empty,
            request.Name,
            request.NasIpAddress,
            request.NasSecret,
            request.NasType,
            request.Location);

        await _nasRepository.AddAsync(nas, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var log = AaaAuditLog.Create(
            _currentTenant.TenantId ?? string.Empty,
            "NasRegistered",
            nasId: nas.Id,
            nasIpAddress: nas.NasIpAddress,
            detail: $"{{\"name\":\"{nas.Name}\",\"nasType\":\"{nas.NasType}\"}}");

        await _logRepository.AddAsync(log, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(nas.Adapt<NasDto>());
    }
}
```

- [ ] **Modify UpdateNasStatusCommandHandler.cs** to inject `IAaaAuditLogRepository` and `ICurrentTenant`, log after save:

```csharp
using Mapster;
using MediatR;
using Obss.AAA.Application.Abstractions;
using Obss.AAA.Application.DTOs;
using Obss.AAA.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.AAA.Application.Commands.UpdateNasStatus;

public sealed class UpdateNasStatusCommandHandler : IRequestHandler<UpdateNasStatusCommand, Result<NasDto>>
{
    private readonly INasRepository _nasRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAaaAuditLogRepository _logRepository;
    private readonly ICurrentTenant _currentTenant;

    public UpdateNasStatusCommandHandler(
        INasRepository nasRepository,
        IUnitOfWork unitOfWork,
        IAaaAuditLogRepository logRepository,
        ICurrentTenant currentTenant)
    {
        _nasRepository = nasRepository;
        _unitOfWork = unitOfWork;
        _logRepository = logRepository;
        _currentTenant = currentTenant;
    }

    public async Task<Result<NasDto>> Handle(UpdateNasStatusCommand request, CancellationToken cancellationToken)
    {
        var nas = await _nasRepository.GetByIdAsync(request.NasId, cancellationToken);

        if (nas is null)
            return Result.Failure<NasDto>(Error.NotFound("NetworkAccessServer", request.NasId));

        switch (request.Status.ToUpperInvariant())
        {
            case "ACTIVE":
                nas.Activate();
                break;
            case "INACTIVE":
                nas.Deactivate();
                break;
            default:
                return Result.Failure<NasDto>(Error.Validation($"Invalid status '{request.Status}'. Valid values are 'Active' or 'Inactive'."));
        }

        await _nasRepository.UpdateAsync(nas, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var log = AaaAuditLog.Create(
            _currentTenant.TenantId ?? string.Empty,
            "NasStatusChanged",
            nasId: nas.Id,
            nasIpAddress: nas.NasIpAddress,
            detail: $"{{\"name\":\"{nas.Name}\",\"newStatus\":\"{request.Status}\"}}");

        await _logRepository.AddAsync(log, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(nas.Adapt<NasDto>());
    }
}
```

Now modify UpdateNasCommandHandler.cs (from Task 4) to also inject log repo and log after save. Add these fields and lines before the `return Result.Success`:

In constructor, add `IAaaAuditLogRepository _logRepository` and `ICurrentTenant _currentTenant`. After `await _unitOfWork.SaveChangesAsync(cancellationToken);` and before the return, add:

```csharp
var log = AaaAuditLog.Create(
    _currentTenant.TenantId ?? string.Empty,
    "NasUpdated",
    nasId: nas.Id,
    nasIpAddress: nas.NasIpAddress,
    detail: $"{{\"name\":\"{nas.Name}\",\"nasType\":\"{nas.NasType}\"}}");

await _logRepository.AddAsync(log, cancellationToken);
await _unitOfWork.SaveChangesAsync(cancellationToken);
```

Also modify DeleteNasCommandHandler.cs (from Task 5) to log `NasDeleted` before the return.

---

### Task 12: New + Modified Endpoints

**Files:**
- Create: `src/Modules/AAA/Obss.AAA.Api/Endpoints/MetricsEndpoints.cs`
- Create: `src/Modules/AAA/Obss.AAA.Api/Endpoints/AuditLogEndpoints.cs`
- Modify: `src/Modules/AAA/Obss.AAA.Api/Endpoints/NasEndpoints.cs`
- Modify: `src/Modules/AAA/Obss.AAA.Api/Endpoints/SessionEndpoints.cs`
- Modify: `src/Modules/AAA/Obss.AAA.Api/Extensions/ServiceCollectionExtensions.cs` (MapAaaEndpoints)

- [ ] **Create MetricsEndpoints.cs**

```csharp
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.AAA.Application.Queries.GetAaaMetrics;
using Obss.SharedKernel.Application.Authorization;

namespace Obss.AAA.Api.Extensions;

public static class MetricsEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/metrics", async (IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetAaaMetricsQuery(), ct);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound();
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Telecom.ServiceRead));
    }
}
```

- [ ] **Create AuditLogEndpoints.cs**

```csharp
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.AAA.Application.Queries.GetAaaLogs;
using Obss.SharedKernel.Application.Authorization;

namespace Obss.AAA.Api.Extensions;

public static class AuditLogEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/logs", async (
            int page,
            int pageSize,
            string? eventType,
            string? username,
            Guid? nasId,
            DateTime? dateFrom,
            DateTime? dateTo,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var query = new GetAaaLogsQuery(
                Page = page > 0 ? page : 1,
                PageSize = pageSize > 0 ? pageSize : 20,
                EventType = eventType,
                Username = username,
                NasId = nasId,
                DateFrom = dateFrom,
                DateTo = dateTo);

            var result = await mediator.Send(query, ct);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound();
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Telecom.ServiceRead));
    }
}
```

- [ ] **Modify NasEndpoints.cs** — add PUT and DELETE. Append before closing `}` of the `Map` method:

```csharp
        group.MapPut("/nas/{id:guid}", async (Guid id, UpdateNasCommand command, IMediator mediator, CancellationToken ct) =>
        {
            command = command with { Id = id };
            var result = await mediator.Send(command, ct);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Telecom.AdapterManage));

        group.MapDelete("/nas/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new DeleteNasCommand(id), ct);
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.NotFound();
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Telecom.AdapterManage));
```

Also add these using statements at the top:
```csharp
using Obss.AAA.Application.Commands.DeleteNas;
using Obss.AAA.Application.Commands.UpdateNas;
```

- [ ] **Modify SessionEndpoints.cs** — add paginated GET `/sessions`. Append before closing `}`:

```csharp
        group.MapGet("/sessions", async (
            int page,
            int pageSize,
            string? status,
            Guid? nasId,
            string? username,
            DateTime? dateFrom,
            DateTime? dateTo,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var query = new GetSessionsQuery(
                Page = page > 0 ? page : 1,
                PageSize = pageSize > 0 ? pageSize : 20,
                Status = status,
                NasId = nasId,
                Username = username,
                DateFrom = dateFrom,
                DateTo = dateTo);

            var result = await mediator.Send(query, ct);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound();
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Telecom.ServiceRead));
```

Add using:
```csharp
using Obss.AAA.Application.Queries.GetSessions;
```

- [ ] **Modify MapAaaEndpoints** in `ServiceCollectionExtensions.cs` — add the new endpoint maps:

After `SessionEndpoints.Map(group);` add:
```csharp
        MetricsEndpoints.Map(group);
        AuditLogEndpoints.Map(group);
```

---

### Task 13: DI Registration + Migration

**Files:**
- Modify: `src/Modules/AAA/Obss.AAA.Api/Extensions/ServiceCollectionExtensions.cs`

- [ ] **Add repository and event handler registrations** to `AddAaaModule()`:

After `services.AddScoped<IRadiusSessionRepository, RadiusSessionRepository>();` add:
```csharp
        services.AddScoped<IAaaAuditLogRepository, AaaAuditLogRepository>();
```

Before `AaaMappingConfig.Configure();` add:
```csharp
        services.AddScoped<INotificationHandler<RadiusSessionStartedDomainEvent>, LogSessionStartedHandler>();
        services.AddScoped<INotificationHandler<RadiusSessionStoppedDomainEvent>, LogSessionStoppedHandler>();
```

Add the required using statements at the top:
```csharp
using Obss.AAA.Domain.Events;
using Obss.AAA.Infrastructure.EventHandlers;
```

- [ ] **Generate EF migration**

Run:
```bash
dotnet ef migrations add AddAaaAuditLog -p src/Modules/AAA/Obss.AAA.Infrastructure -s src/Host/Obss.Host -c AaaDbContext --connection "Host=localhost;Port=5432;Database=obss_aaa;Username=obss_admin;Password=obss_s3cur3_p@ss"
```

---

### Task 14: Build Verification

- [ ] **Build to check all backend changes compile**

```bash
dotnet build Obss.sln --configuration Release 2>&1 | tail -10
```

Expected: `Build succeeded. 0 Warning(s) 0 Error(s)`

---

### Task 15: Frontend DTO Types + Query Keys

**Files:**
- Modify: `src/api/generated/dto.ts`
- Modify: `src/lib/query-keys.ts`

- [ ] **Add AAA types to `src/api/generated/dto.ts`**

Append before the end of the file:

```typescript
// ── AAA ────────────────────────────────────────────────────────────────
export interface NasDto {
  id: string;
  tenantId: string;
  name: string;
  nasIpAddress: string;
  nasType: string;
  status: string;
  location: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface RadiusSessionDto {
  id: string;
  tenantId: string;
  sessionId: string;
  nasId: string;
  username: string;
  framedIpAddress: string | null;
  calledStationId: string;
  callingStationId: string;
  acctSessionTime: number;
  inputOctets: number;
  outputOctets: number;
  sessionStatus: string;
  startedAt: string;
  stoppedAt: string | null;
  updatedAt: string;
}

export interface AaaMetricsDto {
  totalNas: number;
  activeNas: number;
  inactiveNas: number;
  activeSessions: number;
  sessionsToday: number;
  totalInputOctets: number;
  totalOutputOctets: number;
}

export interface AaaAuditLogDto {
  id: string;
  tenantId: string;
  eventType: string;
  username: string | null;
  nasId: string | null;
  nasIpAddress: string | null;
  detail: string | null;
  timestamp: string;
}

export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
}
```

- [ ] **Add AAA query keys to `src/lib/query-keys.ts`**

Find the `queryKeys` object and add a new `aaa` property. Place it in alphabetical order or after the existing keys. Append inside the main object:

```typescript
  aaa: {
    metrics: () => ["aaa", "metrics"],
    nas: {
      all: () => ["aaa", "nas"],
      list: (filters: Record<string, string>) => ["aaa", "nas", "list", filters],
      detail: (id: string) => ["aaa", "nas", "detail", id],
    },
    sessions: {
      all: () => ["aaa", "sessions"],
      list: (filters: Record<string, string>) => ["aaa", "sessions", "list", filters],
      detail: (id: string) => ["aaa", "sessions", "detail", id],
    },
    logs: {
      list: (filters: Record<string, string>) => ["aaa", "logs", "list", filters],
    },
  },
```

---

### Task 16: Frontend API Hooks — Metrics + NAS

**Files:**
- Create: `src/api/hooks/useAaaMetrics.ts`
- Create: `src/api/hooks/useNasDevices.ts`
- Create: `src/api/hooks/useNasDevice.ts`
- Create: `src/api/hooks/useCreateNas.ts`
- Create: `src/api/hooks/useUpdateNas.ts`
- Create: `src/api/hooks/useDeleteNas.ts`
- Create: `src/api/hooks/useUpdateNasStatus.ts`

- [ ] **Create useAaaMetrics.ts**

```typescript
import { useQuery } from "@tanstack/react-query"
import { api } from "@/services/api"
import { queryKeys } from "@/lib/query-keys"
import type { AaaMetricsDto } from "@/api/generated/dto"

export function useAaaMetrics() {
  return useQuery({
    queryKey: queryKeys.aaa.metrics(),
    queryFn: async () => {
      const res = await api.get("/api/v1/aaa/metrics")
      return res.data as AaaMetricsDto
    },
  })
}
```

- [ ] **Create useNasDevices.ts**

```typescript
import { useQuery } from "@tanstack/react-query"
import { api } from "@/services/api"
import { queryKeys } from "@/lib/query-keys"
import type { NasDto, PaginatedResult } from "@/api/generated/dto"

export function useNasDevices(filters: Record<string, string> = {}) {
  return useQuery({
    queryKey: queryKeys.aaa.nas.list(filters),
    queryFn: async () => {
      const params = new URLSearchParams()
      Object.entries(filters).forEach(([k, v]) => { if (v) params.set(k, v) })
      const res = await api.get(`/api/v1/aaa/nas?${params.toString()}`)
      return res.data as PaginatedResult<NasDto>
    },
  })
}
```

- [ ] **Create useNasDevice.ts**

```typescript
import { useQuery } from "@tanstack/react-query"
import { api } from "@/services/api"
import { queryKeys } from "@/lib/query-keys"
import type { NasDto } from "@/api/generated/dto"

export function useNasDevice(id: string) {
  return useQuery({
    queryKey: queryKeys.aaa.nas.detail(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/aaa/nas/${id}`)
      return res.data as NasDto
    },
    enabled: !!id,
  })
}
```

- [ ] **Create useCreateNas.ts**

```typescript
import { useMutation, useQueryClient } from "@tanstack/react-query"
import { api } from "@/services/api"
import { queryKeys } from "@/lib/query-keys"
import type { NasDto } from "@/api/generated/dto"

export function useCreateNas() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: {
      name: string
      nasIpAddress: string
      nasSecret: string
      nasType: string
      location?: string
    }) => {
      const res = await api.post("/api/v1/aaa/nas", data)
      return res.data as NasDto
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.aaa.nas.all() })
      queryClient.invalidateQueries({ queryKey: queryKeys.aaa.metrics() })
    },
  })
}
```

- [ ] **Create useUpdateNas.ts**

```typescript
import { useMutation, useQueryClient } from "@tanstack/react-query"
import { api } from "@/services/api"
import { queryKeys } from "@/lib/query-keys"
import type { NasDto } from "@/api/generated/dto"

export function useUpdateNas() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ id, ...data }: {
      id: string
      name: string
      nasIpAddress: string
      nasSecret: string
      nasType: string
      location?: string
    }) => {
      const res = await api.put(`/api/v1/aaa/nas/${id}`, data)
      return res.data as NasDto
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.aaa.nas.detail(variables.id) })
      queryClient.invalidateQueries({ queryKey: queryKeys.aaa.nas.lists() })
    },
  })
}
```

- [ ] **Create useDeleteNas.ts**

```typescript
import { useMutation, useQueryClient } from "@tanstack/react-query"
import { api } from "@/services/api"
import { queryKeys } from "@/lib/query-keys"

export function useDeleteNas() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      await api.delete(`/api/v1/aaa/nas/${id}`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.aaa.nas.all() })
      queryClient.invalidateQueries({ queryKey: queryKeys.aaa.metrics() })
    },
  })
}
```

- [ ] **Create useUpdateNasStatus.ts**

```typescript
import { useMutation, useQueryClient } from "@tanstack/react-query"
import { api } from "@/services/api"
import { queryKeys } from "@/lib/query-keys"
import type { NasDto } from "@/api/generated/dto"

export function useUpdateNasStatus() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ id, status }: { id: string; status: string }) => {
      const res = await api.patch(`/api/v1/aaa/nas/${id}/status`, { status })
      return res.data as NasDto
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.aaa.nas.detail(variables.id) })
      queryClient.invalidateQueries({ queryKey: queryKeys.aaa.nas.lists() })
    },
  })
}
```

---

### Task 17: Frontend API Hooks — Sessions + Logs

**Files:**
- Create: `src/api/hooks/useSessions.ts`
- Create: `src/api/hooks/useSession.ts`
- Create: `src/api/hooks/useAaaLogs.ts`

- [ ] **Create useSessions.ts**

```typescript
import { useQuery } from "@tanstack/react-query"
import { api } from "@/services/api"
import { queryKeys } from "@/lib/query-keys"
import type { RadiusSessionDto, PaginatedResult } from "@/api/generated/dto"

export function useSessions(filters: Record<string, string> = {}) {
  return useQuery({
    queryKey: queryKeys.aaa.sessions.list(filters),
    queryFn: async () => {
      const params = new URLSearchParams()
      Object.entries(filters).forEach(([k, v]) => { if (v) params.set(k, v) })
      const res = await api.get(`/api/v1/aaa/sessions?${params.toString()}`)
      return res.data as PaginatedResult<RadiusSessionDto>
    },
  })
}
```

- [ ] **Create useSession.ts**

```typescript
import { useQuery } from "@tanstack/react-query"
import { api } from "@/services/api"
import { queryKeys } from "@/lib/query-keys"
import type { RadiusSessionDto } from "@/api/generated/dto"

export function useSession(id: string) {
  return useQuery({
    queryKey: queryKeys.aaa.sessions.detail(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/aaa/sessions/${id}`)
      return res.data as RadiusSessionDto
    },
    enabled: !!id,
  })
}
```

- [ ] **Create useAaaLogs.ts**

```typescript
import { useQuery } from "@tanstack/react-query"
import { api } from "@/services/api"
import { queryKeys } from "@/lib/query-keys"
import type { AaaAuditLogDto, PaginatedResult } from "@/api/generated/dto"

export function useAaaLogs(filters: Record<string, string> = {}) {
  return useQuery({
    queryKey: queryKeys.aaa.logs.list(filters),
    queryFn: async () => {
      const params = new URLSearchParams()
      Object.entries(filters).forEach(([k, v]) => { if (v) params.set(k, v) })
      const res = await api.get(`/api/v1/aaa/logs?${params.toString()}`)
      return res.data as PaginatedResult<AaaAuditLogDto>
    },
  })
}
```

---

### Task 18: Frontend Create NAS Directory + Dashboard Page

**Files:**
- Create: `src/app/aaa/page.tsx`

- [ ] **Create AAA Dashboard page**

```typescript
"use client"

import { PageHeader } from "@/components/shared/PageHeader"
import { MetricCard } from "@/components/shared/MetricCard"
import { DataTable } from "@/components/shared/DataTable"
import { useAaaMetrics } from "@/api/hooks/useAaaMetrics"
import { useAaaLogs } from "@/api/hooks/useAaaLogs"
import { Shield, Server, Activity, Wifi, Database, ArrowUpDown } from "lucide-react"
import type { AaaAuditLogDto } from "@/api/generated/dto"

function formatBytes(bytes: number): string {
  if (bytes === 0) return "0 B"
  const k = 1024
  const sizes = ["B", "KB", "MB", "GB", "TB"]
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return `${(bytes / Math.pow(k, i)).toFixed(1)} ${sizes[i]}`
}

export default function AaaDashboardPage() {
  const { data: metrics, isLoading: metricsLoading } = useAaaMetrics()
  const { data: recentLogs } = useAaaLogs({ page: "1", pageSize: "10" })

  const logColumns = [
    { id: "timestamp", header: "Timestamp", accessorKey: "timestamp" as const },
    { id: "eventType", header: "Event", accessorKey: "eventType" as const },
    { id: "username", header: "Username", accessorKey: "username" as const },
    { id: "nasIpAddress", header: "NAS IP", accessorKey: "nasIpAddress" as const },
  ]

  return (
    <div className="space-y-6">
      <PageHeader title="AAA Dashboard" />

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <MetricCard
          title="Total NAS Devices"
          value={metrics?.totalNas ?? 0}
          icon={Server}
          loading={metricsLoading}
        />
        <MetricCard
          title="Active NAS"
          value={metrics?.activeNas ?? 0}
          icon={Activity}
          loading={metricsLoading}
        />
        <MetricCard
          title="Active Sessions"
          value={metrics?.activeSessions ?? 0}
          icon={Wifi}
          loading={metricsLoading}
        />
        <MetricCard
          title="Sessions Today"
          value={metrics?.sessionsToday ?? 0}
          icon={Database}
          loading={metricsLoading}
        />
      </div>

      <div className="grid gap-4 md:grid-cols-2">
        <MetricCard
          title="Data Transferred (In)"
          value={formatBytes(metrics?.totalInputOctets ?? 0)}
          icon={ArrowUpDown}
          loading={metricsLoading}
        />
        <MetricCard
          title="Data Transferred (Out)"
          value={formatBytes(metrics?.totalOutputOctets ?? 0)}
          icon={ArrowUpDown}
          loading={metricsLoading}
        />
      </div>

      <div>
        <h2 className="text-lg font-semibold mb-3">Recent Activity</h2>
        <DataTable<AaaAuditLogDto>
          columns={logColumns}
          data={recentLogs?.items ?? []}
          rowKey={(row) => row.id}
          loading={false}
          emptyTitle="No recent activity"
        />
      </div>
    </div>
  )
}
```

---

### Task 19: Frontend NAS List Page

**Files:**
- Create: `src/app/aaa/nas/page.tsx`

- [ ] **Create NAS List page**

```typescript
"use client"

import { useState } from "react"
import { useRouter } from "next/navigation"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { SearchBar } from "@/components/shared/SearchBar"
import { FilterBar } from "@/components/shared/FilterBar"
import { useNasDevices } from "@/api/hooks/useNasDevices"
import { useDeleteNas } from "@/api/hooks/useDeleteNas"
import type { NasDto, PaginatedResult } from "@/api/generated/dto"
import { toast } from "@/components/ui/toast"
import { Button } from "@/components/ui/button"
import { Trash2 } from "lucide-react"

const nasTypeOptions = [
  { value: "", label: "All Types" },
  { value: "BRAS", label: "BRAS" },
  { value: "BNG", label: "BNG" },
  { value: "WLC", label: "WLC" },
  { value: "VSAT", label: "VSAT" },
  { value: "UAG", label: "UAG" },
]

const statusOptions = [
  { value: "", label: "All Status" },
  { value: "Active", label: "Active" },
  { value: "Inactive", label: "Inactive" },
]

export default function NasListPage() {
  const router = useRouter()
  const [search, setSearch] = useState("")
  const [nasType, setNasType] = useState("")
  const [status, setStatus] = useState("")
  const [page, setPage] = useState(1)
  const deleteNas = useDeleteNas()

  const filters: Record<string, string> = { page: String(page), pageSize: "20" }
  if (nasType) filters.nasType = nasType
  if (status) filters.status = status

  const { data, isLoading, error } = useNasDevices(filters)
  const paginated = data as PaginatedResult<NasDto> | undefined

  const handleDelete = async (id: string, name: string) => {
    if (!confirm(`Delete NAS device "${name}"?`)) return
    await deleteNas.mutateAsync(id)
    toast({ title: "NAS deleted", description: `"${name}" has been removed.` })
  }

  const columns = [
    { id: "name", header: "Name", accessorKey: "name" as const, sortable: true },
    { id: "nasIpAddress", header: "IP Address", accessorKey: "nasIpAddress" as const },
    { id: "nasType", header: "Type", accessorKey: "nasType" as const },
    {
      id: "status",
      header: "Status",
      cell: (row: NasDto) => <StatusBadge status={row.status} />,
    },
    { id: "location", header: "Location", accessorKey: "location" as const },
    { id: "createdAt", header: "Created", accessorKey: "createdAt" as const },
  ]

  const bulkActions = [
    {
      label: "Delete Selected",
      icon: Trash2,
      onClick: async (ids: string[]) => {
        for (const id of ids) {
          await deleteNas.mutateAsync(id)
        }
        toast({ title: "Deleted", description: `${ids.length} NAS device(s) removed.` })
      },
    },
  ]

  return (
    <div className="space-y-4">
      <PageHeader
        title="NAS Devices"
        createLabel="Register NAS"
        onCreateClick={() => router.push("/aaa/nas/new")}
      />
      <div className="flex gap-2">
        <SearchBar value={search} onChange={setSearch} placeholder="Search NAS devices..." />
        <FilterBar options={nasTypeOptions} value={nasType} onChange={setNasType} />
        <FilterBar options={statusOptions} value={status} onChange={setStatus} />
      </div>
      <DataTable<NasDto>
        columns={columns}
        data={paginated?.items ?? []}
        loading={isLoading}
        error={error?.message}
        rowKey={(row) => row.id}
        onRowClick={(row) => router.push(`/aaa/nas/${row.id}`)}
        emptyTitle="No NAS devices registered"
        pagination={{
          page,
          pageSize: 20,
          total: paginated?.totalCount ?? 0,
          onPageChange: setPage,
          onPageSizeChange: () => {},
        }}
        bulkActions={bulkActions}
      />
    </div>
  )
}
```

---

### Task 20: Frontend NAS Create Page

**Files:**
- Create: `src/app/aaa/nas/new/page.tsx`

- [ ] **Create NAS Create page**

```typescript
"use client"

import { useRouter } from "next/navigation"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { FormPageLayout } from "@/forms/FormPageLayout"
import { FormSection } from "@/forms/FormSection"
import { FormField, FormSelectField } from "@/forms/FormField"
import { FormActions } from "@/forms/FormActions"
import { FormErrorSummary } from "@/forms/FormErrorSummary"
import { useCreateNas } from "@/api/hooks/useCreateNas"
import { toast } from "@/components/ui/toast"

const schema = z.object({
  name: z.string().min(1, "Name is required").max(200),
  nasIpAddress: z.string().min(1, "IP address is required").max(45),
  nasSecret: z.string().min(8, "Secret must be at least 8 characters").max(500),
  nasType: z.string().min(1, "Type is required"),
  location: z.string().max(500).optional(),
})
type FormData = z.infer<typeof schema>

const nasTypeOptions = [
  { value: "BRAS", label: "BRAS" },
  { value: "BNG", label: "BNG" },
  { value: "WLC", label: "WLC" },
  { value: "VSAT", label: "VSAT" },
  { value: "UAG", label: "UAG" },
]

export default function CreateNasPage() {
  const router = useRouter()
  const createNas = useCreateNas()
  const { register, handleSubmit, formState: { errors }, watch, setValue } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: { nasType: "BRAS" },
  })

  const onSubmit = async (data: FormData) => {
    try {
      const result = await createNas.mutateAsync(data)
      toast({ title: "NAS registered", description: `"${result.name}" has been added.` })
      router.push(`/aaa/nas/${result.id}`)
    } catch {
      toast({ title: "Error", description: "Failed to register NAS device.", variant: "destructive" })
    }
  }

  return (
    <FormPageLayout title="Register NAS" backHref="/aaa/nas" onSubmit={handleSubmit(onSubmit)}>
      <FormErrorSummary errors={errors} />
      <FormSection title="Device Details">
        <FormField label="Name" error={errors.name} registration={register("name")} required placeholder="e.g. Sanaa-BRAS-01" />
        <FormField label="IP Address" error={errors.nasIpAddress} registration={register("nasIpAddress")} required placeholder="e.g. 10.0.0.1" />
        <FormField label="Secret" error={errors.nasSecret} registration={register("nasSecret")} required type="password" placeholder="At least 8 characters" />
        <FormSelectField label="Type" error={errors.nasType} options={nasTypeOptions} value={watch("nasType")} onValueChange={(v) => setValue("nasType", v)} required />
        <FormField label="Location" error={errors.location} registration={register("location")} placeholder="e.g. Sanaa Data Center" />
      </FormSection>
      <FormActions backHref="/aaa/nas" loading={createNas.isPending} />
    </FormPageLayout>
  )
}
```

---

### Task 21: Frontend NAS Detail + Edit Pages

**Files:**
- Create: `src/app/aaa/nas/[id]/page.tsx`
- Create: `src/app/aaa/nas/[id]/edit/page.tsx`

- [ ] **Create NAS Detail page** (`src/app/aaa/nas/[id]/page.tsx`)

```typescript
"use client"

import { useRouter } from "next/navigation"
import { EntityHeader } from "@/components/shared/EntityHeader"
import { EntityTabs } from "@/components/shared/EntityTabs"
import { EntityMetadata } from "@/components/shared/EntityMetadata"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { DataTable } from "@/components/shared/DataTable"
import { useNasDevice } from "@/api/hooks/useNasDevice"
import { useDeleteNas } from "@/api/hooks/useDeleteNas"
import { useUpdateNasStatus } from "@/api/hooks/useUpdateNasStatus"
import { useSessions } from "@/api/hooks/useSessions"
import { useAaaLogs } from "@/api/hooks/useAaaLogs"
import { toast } from "@/components/ui/toast"
import type { NasDto, RadiusSessionDto, AaaAuditLogDto } from "@/api/generated/dto"

export default function NasDetailPage({ params }: { params: { id: string } }) {
  const router = useRouter()
  const { data: nas, isLoading } = useNasDevice(params.id)
  const deleteNas = useDeleteNas()
  const updateStatus = useUpdateNasStatus()
  const { data: sessions } = useSessions({ nasId: params.id, pageSize: "10" })
  const { data: logs } = useAaaLogs({ nasId: params.id, pageSize: "10" })

  const handleDelete = async () => {
    if (!confirm(`Delete NAS "${nas?.name}"?`)) return
    await deleteNas.mutateAsync(params.id)
    toast({ title: "Deleted", description: "NAS device removed." })
    router.push("/aaa/nas")
  }

  const handleToggleStatus = async () => {
    const newStatus = nas?.status === "Active" ? "Inactive" : "Active"
    await updateStatus.mutateAsync({ id: params.id, status: newStatus })
    toast({ title: "Status updated", description: `NAS is now ${newStatus}.` })
  }

  if (isLoading || !nas) return null

  const overviewFields = [
    { label: "Name", value: nas.name },
    { label: "IP Address", value: nas.nasIpAddress },
    { label: "Type", value: nas.nasType },
    { label: "Status", value: <StatusBadge status={nas.status} /> },
    { label: "Location", value: nas.location ?? "—" },
    { label: "Created", value: new Date(nas.createdAt).toLocaleString() },
    { label: "Updated", value: new Date(nas.updatedAt).toLocaleString() },
  ]

  const sessionColumns = [
    { id: "username", header: "Username", accessorKey: "username" as const },
    { id: "framedIpAddress", header: "Framed IP", accessorKey: "framedIpAddress" as const },
    { id: "sessionStatus", header: "Status", cell: (row: RadiusSessionDto) => <StatusBadge status={row.sessionStatus} /> },
    { id: "startedAt", header: "Started", accessorKey: "startedAt" as const },
  ]

  const logColumns = [
    { id: "timestamp", header: "Timestamp", accessorKey: "timestamp" as const },
    { id: "eventType", header: "Event", accessorKey: "eventType" as const },
    { id: "username", header: "Username", accessorKey: "username" as const },
  ]

  const tabs = [
    {
      value: "overview",
      label: "Overview",
      content: <EntityMetadata fields={overviewFields} columns={2} />,
    },
    {
      value: "sessions",
      label: "Sessions",
      content: (
        <DataTable<RadiusSessionDto>
          columns={sessionColumns}
          data={sessions?.items ?? []}
          rowKey={(row) => row.id}
          emptyTitle="No sessions for this NAS"
        />
      ),
    },
    {
      value: "logs",
      label: "Audit Log",
      content: (
        <DataTable<AaaAuditLogDto>
          columns={logColumns}
          data={logs?.items ?? []}
          rowKey={(row) => row.id}
          emptyTitle="No audit entries for this NAS"
        />
      ),
    },
  ]

  return (
    <div className="space-y-4">
      <EntityHeader
        title={nas.name}
        status={<StatusBadge status={nas.status} />}
        backHref="/aaa/nas"
        onEdit={() => router.push(`/aaa/nas/${params.id}/edit`)}
        onDelete={handleDelete}
        actions={[
          {
            label: nas.status === "Active" ? "Deactivate" : "Activate",
            onClick: handleToggleStatus,
            variant: "outline",
          },
        ]}
      />
      <EntityTabs tabs={tabs} />
    </div>
  )
}
```

- [ ] **Create NAS Edit page** (`src/app/aaa/nas/[id]/edit/page.tsx`)

```typescript
"use client"

import { useRouter } from "next/navigation"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { FormPageLayout } from "@/forms/FormPageLayout"
import { FormSection } from "@/forms/FormSection"
import { FormField, FormSelectField } from "@/forms/FormField"
import { FormActions } from "@/forms/FormActions"
import { FormErrorSummary } from "@/forms/FormErrorSummary"
import { useNasDevice } from "@/api/hooks/useNasDevice"
import { useUpdateNas } from "@/api/hooks/useUpdateNas"
import { toast } from "@/components/ui/toast"

const schema = z.object({
  name: z.string().min(1, "Name is required").max(200),
  nasIpAddress: z.string().min(1, "IP address is required").max(45),
  nasSecret: z.string().min(8, "Secret must be at least 8 characters").max(500),
  nasType: z.string().min(1, "Type is required"),
  location: z.string().max(500).optional(),
})
type FormData = z.infer<typeof schema>

const nasTypeOptions = [
  { value: "BRAS", label: "BRAS" },
  { value: "BNG", label: "BNG" },
  { value: "WLC", label: "WLC" },
  { value: "VSAT", label: "VSAT" },
  { value: "UAG", label: "UAG" },
]

export default function EditNasPage({ params }: { params: { id: string } }) {
  const router = useRouter()
  const { data: nas, isLoading } = useNasDevice(params.id)
  const updateNas = useUpdateNas()
  const { register, handleSubmit, formState: { errors }, watch, setValue } = useForm<FormData>({
    resolver: zodResolver(schema),
    values: nas ? {
      name: nas.name,
      nasIpAddress: nas.nasIpAddress,
      nasSecret: "",
      nasType: nas.nasType,
      location: nas.location ?? "",
    } : undefined,
  })

  const onSubmit = async (data: FormData) => {
    try {
      await updateNas.mutateAsync({ id: params.id, ...data })
      toast({ title: "NAS updated", description: "Changes saved." })
      router.push(`/aaa/nas/${params.id}`)
    } catch {
      toast({ title: "Error", description: "Failed to update NAS.", variant: "destructive" })
    }
  }

  if (isLoading) return null

  return (
    <FormPageLayout title="Edit NAS" backHref={`/aaa/nas/${params.id}`} onSubmit={handleSubmit(onSubmit)}>
      <FormErrorSummary errors={errors} />
      <FormSection title="Device Details">
        <FormField label="Name" error={errors.name} registration={register("name")} required />
        <FormField label="IP Address" error={errors.nasIpAddress} registration={register("nasIpAddress")} required />
        <FormField label="New Secret" error={errors.nasSecret} registration={register("nasSecret")} required type="password" placeholder="Leave blank to keep existing" />
        <FormSelectField label="Type" error={errors.nasType} options={nasTypeOptions} value={watch("nasType")} onValueChange={(v) => setValue("nasType", v)} required />
        <FormField label="Location" error={errors.location} registration={register("location")} />
      </FormSection>
      <FormActions backHref={`/aaa/nas/${params.id}`} loading={updateNas.isPending} />
    </FormPageLayout>
  )
}
```

---

### Task 22: Frontend Sessions List Page

**Files:**
- Create: `src/app/aaa/sessions/page.tsx`

- [ ] **Create Sessions List page**

```typescript
"use client"

import { useState } from "react"
import { useRouter } from "next/navigation"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { SearchBar } from "@/components/shared/SearchBar"
import { FilterBar } from "@/components/shared/FilterBar"
import { useSessions } from "@/api/hooks/useSessions"
import type { RadiusSessionDto, PaginatedResult } from "@/api/generated/dto"

const statusOptions = [
  { value: "", label: "All Status" },
  { value: "Active", label: "Active" },
  { value: "Stopped", label: "Stopped" },
  { value: "Interim", label: "Interim" },
]

function formatDuration(seconds: number): string {
  const h = Math.floor(seconds / 3600)
  const m = Math.floor((seconds % 3600) / 60)
  const s = seconds % 60
  return `${h}h ${m}m ${s}s`
}

function formatBytes(bytes: number): string {
  if (bytes === 0) return "0 B"
  const k = 1024
  const sizes = ["B", "KB", "MB", "GB", "TB"]
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return `${(bytes / Math.pow(k, i)).toFixed(1)} ${sizes[i]}`
}

export default function SessionsListPage() {
  const router = useRouter()
  const [search, setSearch] = useState("")
  const [status, setStatus] = useState("Active")
  const [page, setPage] = useState(1)

  const filters: Record<string, string> = { page: String(page), pageSize: "20" }
  if (status) filters.status = status
  if (search) filters.username = search

  const { data, isLoading, error } = useSessions(filters)
  const paginated = data as PaginatedResult<RadiusSessionDto> | undefined

  const columns = [
    { id: "username", header: "Username", accessorKey: "username" as const, sortable: true },
    { id: "framedIpAddress", header: "Framed IP", accessorKey: "framedIpAddress" as const },
    { id: "sessionStatus", header: "Status", cell: (row: RadiusSessionDto) => <StatusBadge status={row.sessionStatus} /> },
    { id: "acctSessionTime", header: "Duration", cell: (row: RadiusSessionDto) => formatDuration(row.acctSessionTime) },
    { id: "inputOctets", header: "RX", cell: (row: RadiusSessionDto) => formatBytes(row.inputOctets) },
    { id: "outputOctets", header: "TX", cell: (row: RadiusSessionDto) => formatBytes(row.outputOctets) },
    { id: "startedAt", header: "Started", accessorKey: "startedAt" as const, sortable: true },
  ]

  return (
    <div className="space-y-4">
      <PageHeader title="RADIUS Sessions" />
      <div className="flex gap-2">
        <SearchBar value={search} onChange={setSearch} placeholder="Search by username..." />
        <FilterBar options={statusOptions} value={status} onChange={setStatus} />
      </div>
      <DataTable<RadiusSessionDto>
        columns={columns}
        data={paginated?.items ?? []}
        loading={isLoading}
        error={error?.message}
        rowKey={(row) => row.id}
        onRowClick={(row) => router.push(`/aaa/sessions/${row.id}`)}
        emptyTitle="No sessions found"
        pagination={{
          page,
          pageSize: 20,
          total: paginated?.totalCount ?? 0,
          onPageChange: setPage,
          onPageSizeChange: () => {},
        }}
      />
    </div>
  )
}
```

---

### Task 23: Frontend Session Detail Page

**Files:**
- Create: `src/app/aaa/sessions/[id]/page.tsx`

- [ ] **Create Session Detail page**

```typescript
"use client"

import { EntityHeader } from "@/components/shared/EntityHeader"
import { EntityMetadata } from "@/components/shared/EntityMetadata"
import { EntityTabs } from "@/components/shared/EntityTabs"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { DataTable } from "@/components/shared/DataTable"
import { useSession } from "@/api/hooks/useSession"
import { useAaaLogs } from "@/api/hooks/useAaaLogs"
import type { AaaAuditLogDto } from "@/api/generated/dto"

function formatDuration(seconds: number): string {
  const h = Math.floor(seconds / 3600)
  const m = Math.floor((seconds % 3600) / 60)
  const s = seconds % 60
  return `${h}h ${m}m ${s}s`
}

function formatBytes(bytes: number): string {
  if (bytes === 0) return "0 B"
  const k = 1024
  const sizes = ["B", "KB", "MB", "GB", "TB"]
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return `${(bytes / Math.pow(k, i)).toFixed(1)} ${sizes[i]}`
}

export default function SessionDetailPage({ params }: { params: { id: string } }) {
  const { data: session, isLoading } = useSession(params.id)
  const { data: logs } = useAaaLogs({ username: session?.username, pageSize: "20" })

  if (isLoading || !session) return null

  const overviewFields = [
    { label: "Session ID", value: session.sessionId },
    { label: "Username", value: session.username },
    { label: "NAS ID", value: session.nasId },
    { label: "Framed IP", value: session.framedIpAddress ?? "—" },
    { label: "Called Station", value: session.calledStationId },
    { label: "Calling Station", value: session.callingStationId },
    { label: "Status", value: <StatusBadge status={session.sessionStatus} /> },
    { label: "Duration", value: formatDuration(session.acctSessionTime) },
    { label: "Data Received", value: formatBytes(session.inputOctets) },
    { label: "Data Sent", value: formatBytes(session.outputOctets) },
    { label: "Started", value: new Date(session.startedAt).toLocaleString() },
    { label: "Stopped", value: session.stoppedAt ? new Date(session.stoppedAt).toLocaleString() : "—" },
    { label: "Last Updated", value: new Date(session.updatedAt).toLocaleString() },
  ]

  const logColumns = [
    { id: "timestamp", header: "Timestamp", accessorKey: "timestamp" as const },
    { id: "eventType", header: "Event", accessorKey: "eventType" as const },
    { id: "nasIpAddress", header: "NAS IP", accessorKey: "nasIpAddress" as const },
  ]

  const tabs = [
    {
      value: "overview",
      label: "Overview",
      content: <EntityMetadata fields={overviewFields} columns={2} />,
    },
    {
      value: "logs",
      label: "Audit Log",
      content: (
        <DataTable<AaaAuditLogDto>
          columns={logColumns}
          data={logs?.items ?? []}
          rowKey={(row) => row.id}
          emptyTitle="No audit entries for this session"
        />
      ),
    },
  ]

  return (
    <div className="space-y-4">
      <EntityHeader title={`Session ${session.sessionId.slice(0, 8)}...`} status={<StatusBadge status={session.sessionStatus} />} backHref="/aaa/sessions" />
      <EntityTabs tabs={tabs} />
    </div>
  )
}
```

---

### Task 24: Frontend Audit Logs Page

**Files:**
- Create: `src/app/aaa/logs/page.tsx`

- [ ] **Create Audit Logs page**

```typescript
"use client"

import { useState } from "react"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { SearchBar } from "@/components/shared/SearchBar"
import { FilterBar } from "@/components/shared/FilterBar"
import { useAaaLogs } from "@/api/hooks/useAaaLogs"
import type { AaaAuditLogDto, PaginatedResult } from "@/api/generated/dto"

const eventTypeOptions = [
  { value: "", label: "All Events" },
  { value: "AuthenticationSuccess", label: "Auth Success" },
  { value: "AuthenticationFailure", label: "Auth Failure" },
  { value: "AccountingStart", label: "Accounting Start" },
  { value: "AccountingStop", label: "Accounting Stop" },
  { value: "AccountingInterim", label: "Accounting Interim" },
  { value: "NasRegistered", label: "NAS Registered" },
  { value: "NasUpdated", label: "NAS Updated" },
  { value: "NasDeleted", label: "NAS Deleted" },
  { value: "NasStatusChanged", label: "NAS Status Changed" },
]

const eventColorMap: Record<string, string> = {
  AuthenticationSuccess: "success",
  AuthenticationFailure: "error",
  AccountingStart: "info",
  AccountingStop: "warning",
  AccountingInterim: "info",
  NasRegistered: "success",
  NasUpdated: "info",
  NasDeleted: "error",
  NasStatusChanged: "warning",
}

export default function AuditLogsPage() {
  const [search, setSearch] = useState("")
  const [eventType, setEventType] = useState("")
  const [page, setPage] = useState(1)

  const filters: Record<string, string> = { page: String(page), pageSize: "50" }
  if (eventType) filters.eventType = eventType
  if (search) filters.username = search

  const { data, isLoading, error } = useAaaLogs(filters)
  const paginated = data as PaginatedResult<AaaAuditLogDto> | undefined

  const columns = [
    { id: "timestamp", header: "Timestamp", accessorKey: "timestamp" as const, sortable: true },
    {
      id: "eventType",
      header: "Event Type",
      cell: (row: AaaAuditLogDto) => (
        <StatusBadge status={row.eventType} color={eventColorMap[row.eventType] ?? "default"} />
      ),
    },
    { id: "username", header: "Username", accessorKey: "username" as const },
    { id: "nasIpAddress", header: "NAS IP", accessorKey: "nasIpAddress" as const },
    { id: "detail", header: "Detail", accessorKey: "detail" as const },
  ]

  return (
    <div className="space-y-4">
      <PageHeader title="Audit Logs" />
      <div className="flex gap-2">
        <SearchBar value={search} onChange={setSearch} placeholder="Search by username..." />
        <FilterBar options={eventTypeOptions} value={eventType} onChange={setEventType} />
      </div>
      <DataTable<AaaAuditLogDto>
        columns={columns}
        data={paginated?.items ?? []}
        loading={isLoading}
        error={error?.message}
        rowKey={(row) => row.id}
        emptyTitle="No audit log entries found"
        pagination={{
          page,
          pageSize: 50,
          total: paginated?.totalCount ?? 0,
          onPageChange: setPage,
          onPageSizeChange: () => {},
        }}
      />
    </div>
  )
}
```

---

### Task 25: Frontend Sidebar + Breadcrumb

**Files:**
- Modify: `src/components/shared/ModuleSidebar.tsx`
- Modify: `src/components/shared/BreadcrumbBuilder.tsx`

- [ ] **Add AAA expandable sub-menu to ModuleSidebar.tsx**

In the `modules` array, the existing entry `{ href: "/network", label: "Network", icon: Cable }` is at line 64-65. Replace it with a similar expandable sub-menu pattern used by Service Inventory and Collections.

Remove the "Network" flat entry and add an expandable AAA section between Tickets and Network. Find the closing `}` of the tickets entry and the opening `{` of the /network entry. Replace the /network entry with:

```typescript
  // { href: "/network", label: "Network", icon: Cable },  // removed — replaced by AAA + new Network below
```

Then after the closing `}` of the modules map in the JSX (before the `{!collapsed && (` for Service Inventory), add the expandable AAA sub-menu:

```typescript
        {!collapsed && (
          <div>
            <button
              onClick={() => setAaaOpen(!aaaOpen)}
              className={cn(
                "flex w-full items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors hover:bg-accent hover:text-accent-foreground",
                pathname.startsWith("/aaa") ? "bg-accent text-accent-foreground" : "text-muted-foreground"
              )}
            >
              <Shield className="h-4 w-4 shrink-0" />
              <span className="flex-1 text-left">AAA</span>
              <ChevronDown
                className={cn(
                  "h-4 w-4 transition-transform",
                  aaaOpen && "rotate-180"
                )}
              />
            </button>
            {aaaOpen && (
              <div className="ml-2 space-y-1 border-l pl-2">
                <Link
                  href="/aaa"
                  className={cn(
                    "flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors hover:bg-accent hover:text-accent-foreground",
                    pathname === "/aaa" ? "bg-accent text-accent-foreground" : "text-muted-foreground"
                  )}
                >
                  <LayoutDashboard className="h-4 w-4 shrink-0" />
                  <span>Dashboard</span>
                </Link>
                <Link
                  href="/aaa/nas"
                  className={cn(
                    "flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors hover:bg-accent hover:text-accent-foreground",
                    pathname.startsWith("/aaa/nas") ? "bg-accent text-accent-foreground" : "text-muted-foreground"
                  )}
                >
                  <Server className="h-4 w-4 shrink-0" />
                  <span>NAS Devices</span>
                </Link>
                <Link
                  href="/aaa/sessions"
                  className={cn(
                    "flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors hover:bg-accent hover:text-accent-foreground",
                    pathname.startsWith("/aaa/sessions") ? "bg-accent text-accent-foreground" : "text-muted-foreground"
                  )}
                >
                  <Wifi className="h-4 w-4 shrink-0" />
                  <span>Sessions</span>
                </Link>
                <Link
                  href="/aaa/logs"
                  className={cn(
                    "flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors hover:bg-accent hover:text-accent-foreground",
                    pathname.startsWith("/aaa/logs") ? "bg-accent text-accent-foreground" : "text-muted-foreground"
                  )}
                >
                  <ScrollText className="h-4 w-4 shrink-0" />
                  <span>Audit Logs</span>
                </Link>
              </div>
            )}
          </div>
        )}
```

Also add a `useState` for `aaaOpen` near the other state declarations:
```typescript
  const [aaaOpen, setAaaOpen] = useState(false)
```

Add the import for `Server`, `Wifi` (new imports needed) — these are already imported from `lucide-react` in the existing sidebar (check: yes, `Server` and `Wifi` are not in the current import list). Add them to the import from `lucide-react`:
```typescript
import {
  // ... existing imports ...
  Server,
  Wifi,
} from "lucide-react"
```

The `/network` flat item can remain (it points to the existing Network module). Or remove it and let the AAA section replace it. Keep the existing `/network` entry but put it after the AAA expandable section.

- [ ] **Add breadcrumb labels to BreadcrumbBuilder.tsx**

Find the `labelMap` object and add:
```typescript
  "/aaa": "AAA",
  "/aaa/nas": "NAS Devices",
  "/aaa/sessions": "Sessions",
  "/aaa/logs": "Audit Logs",
```

---

### Task 26: Final Build Verification

- [ ] **Backend build**

```bash
dotnet build Obss.sln --configuration Release 2>&1 | tail -5
```

Expected: `Build succeeded. 0 Warning(s) 0 Error(s)`

- [ ] **Frontend build**

```bash
bun run build 2>&1 | tail -10
```

Expected: `✓ Built in Xs` with no TypeScript errors.

---

## File Index

### Backend (New Files)
| # | Path | Purpose |
|---|------|---------|
| 1 | `src/.../Application/Contracts/PaginatedResult.cs` | Generic paginated result type |
| 2 | `src/.../Application/DTOs/AaaMetricsDto.cs` | Dashboard metrics DTO |
| 3 | `src/.../Application/DTOs/AaaAuditLogDto.cs` | Audit log entry DTO |
| 4 | `src/.../Domain/ValueObjects/AaaEventType.cs` | Event type enum |
| 5 | `src/.../Domain/Entities/AaaAuditLog.cs` | Audit log entity |
| 6 | `src/.../Infrastructure/.../AaaAuditLogConfiguration.cs` | EF config |
| 7 | `src/.../Application/Abstractions/IAaaAuditLogRepository.cs` | Log repository interface |
| 8 | `src/.../Infrastructure/.../AaaAuditLogRepository.cs` | Log repository implementation |
| 9 | `src/.../Application/Commands/UpdateNas/UpdateNasCommand.cs` | Update NAS command |
| 10 | `src/.../Application/Commands/UpdateNas/UpdateNasCommandHandler.cs` | Update NAS handler |
| 11 | `src/.../Application/Commands/UpdateNas/UpdateNasCommandValidator.cs` | Update NAS validator |
| 12 | `src/.../Application/Commands/DeleteNas/DeleteNasCommand.cs` | Delete NAS command |
| 13 | `src/.../Application/Commands/DeleteNas/DeleteNasCommandHandler.cs` | Delete NAS handler |
| 14 | `src/.../Application/Queries/GetAaaMetrics/GetAaaMetricsQuery.cs` | Metrics query |
| 15 | `src/.../Application/Queries/GetAaaMetrics/GetAaaMetricsQueryHandler.cs` | Metrics handler |
| 16 | `src/.../Application/Queries/GetSessions/GetSessionsQuery.cs` | Sessions query |
| 17 | `src/.../Application/Queries/GetSessions/GetSessionsQueryHandler.cs` | Sessions handler |
| 18 | `src/.../Application/Queries/GetAaaLogs/GetAaaLogsQuery.cs` | Logs query |
| 19 | `src/.../Application/Queries/GetAaaLogs/GetAaaLogsQueryHandler.cs` | Logs handler |
| 20 | `src/.../Infrastructure/EventHandlers/LogSessionStartedHandler.cs` | Domain event handler |
| 21 | `src/.../Infrastructure/EventHandlers/LogSessionStoppedHandler.cs` | Domain event handler |
| 22 | `src/.../Api/Endpoints/MetricsEndpoints.cs` | Metrics endpoint |
| 23 | `src/.../Api/Endpoints/AuditLogEndpoints.cs` | Logs endpoint |

### Backend (Modified Files)
| # | Path | Change |
|---|------|--------|
| 1 | `AaaDbContext.cs` | Add `DbSet<AaaAuditLog>` |
| 2 | `GetAllNasDevicesQuery.cs` | Add pagination + filters |
| 3 | `GetAllNasDevicesQueryHandler.cs` | Return `PaginatedResult<NasDto>` |
| 4 | `NasEndpoints.cs` | Add PUT + DELETE |
| 5 | `SessionEndpoints.cs` | Add paginated GET |
| 6 | `ServiceCollectionExtensions.cs` | Register repos, event handlers, map new endpoints |
| 7 | `RegisterNasCommandHandler.cs` | Add audit logging |
| 8 | `UpdateNasStatusCommandHandler.cs` | Add audit logging |

### Frontend (New Files)
| # | Path | Purpose |
|---|------|---------|
| 1 | `src/api/hooks/useAaaMetrics.ts` | Dashboard metrics |
| 2 | `src/api/hooks/useNasDevices.ts` | NAS list |
| 3 | `src/api/hooks/useNasDevice.ts` | NAS detail |
| 4 | `src/api/hooks/useCreateNas.ts` | Create NAS |
| 5 | `src/api/hooks/useUpdateNas.ts` | Update NAS |
| 6 | `src/api/hooks/useDeleteNas.ts` | Delete NAS |
| 7 | `src/api/hooks/useUpdateNasStatus.ts` | Toggle NAS status |
| 8 | `src/api/hooks/useSessions.ts` | Session list |
| 9 | `src/api/hooks/useSession.ts` | Session detail |
| 10 | `src/api/hooks/useAaaLogs.ts` | Audit logs |
| 11 | `src/app/aaa/page.tsx` | Dashboard |
| 12 | `src/app/aaa/nas/page.tsx` | NAS list |
| 13 | `src/app/aaa/nas/new/page.tsx` | Create NAS |
| 14 | `src/app/aaa/nas/[id]/page.tsx` | NAS detail |
| 15 | `src/app/aaa/nas/[id]/edit/page.tsx` | Edit NAS |
| 16 | `src/app/aaa/sessions/page.tsx` | Session list |
| 17 | `src/app/aaa/sessions/[id]/page.tsx` | Session detail |
| 18 | `src/app/aaa/logs/page.tsx` | Audit logs |

### Frontend (Modified Files)
| # | Path | Change |
|---|------|--------|
| 1 | `src/api/generated/dto.ts` | Add AAA types |
| 2 | `src/lib/query-keys.ts` | Add AAA query keys |
| 3 | `src/components/shared/ModuleSidebar.tsx` | Add AAA sub-menu |
| 4 | `src/components/shared/BreadcrumbBuilder.tsx` | Add AAA labels |
