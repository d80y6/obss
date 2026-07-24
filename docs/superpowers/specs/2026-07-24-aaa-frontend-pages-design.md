# AAA Frontend Pages — Design Spec

## Overview

Add frontend management pages for the AAA (Authentication, Authorization, Accounting) module, covering NAS device management, RADIUS session monitoring, and audit log review. Requires 5 new backend endpoints and an `AaaAuditLog` entity.

## New Backend API Endpoints

| Method | Endpoint | Purpose |
|--------|----------|---------|
| `GET` | `/api/v1/aaa/metrics` | Dashboard metrics: total NAS, active/inactive counts, active sessions, sessions today, total data transferred |
| `PUT` | `/api/v1/aaa/nas/{id}` | Update NAS settings |
| `DELETE` | `/api/v1/aaa/nas/{id}` | Remove a NAS device |
| `GET` | `/api/v1/aaa/sessions` | Paginated session list with filters (status, nasId, username, dateFrom, dateTo) |
| `GET` | `/api/v1/aaa/logs` | Paginated audit log with filters (eventType, username, nasId, dateFrom, dateTo) |

Existing endpoints remain: `POST /nas` (register), `PATCH /nas/{id}/status`, `GET /nas`, `GET /nas/{id}`, `GET /sessions/active`, `GET /sessions/{id}`, `GET /sessions/by-user/{username}`.

The `GetAllNasDevices` handler is extended to support pagination and `NasType` filter.

### Backend Changes

#### New Entity: `AaaAuditLog`
- Domain: `Obss.AAA.Domain.Entities.AaaAuditLog` — `AggregateRoot<Guid>`, `ITenantEntity`
- Fields:
  - `EventType` — enum `AaaEventType`: `AuthenticationSuccess`, `AuthenticationFailure`, `AccountingStart`, `AccountingStop`, `AccountingInterim`, `NasRegistered`, `NasUpdated`, `NasDeleted`, `NasStatusChanged`
  - `Username` (string?, nullable for NAS events)
  - `NasId` (Guid?)
  - `NasIpAddress` (string?)
  - `Detail` (string? — JSON payload)
  - `Timestamp` (DateTime)
- Repository: `IAaaAuditLogRepository` with `AddAsync`, `GetPaginatedAsync`
- Event handler: listens to `RadiusSessionStartedDomainEvent`, `RadiusSessionStoppedDomainEvent` + NAS command events to persist log entries

#### New Commands/Queries
- **UpdateNasCommand** + handler + validator — updates NAS name/IP/secret/type/location, logs `NasUpdated`
- **DeleteNasCommand** + handler — removes NAS, logs `NasDeleted`
- **GetAaaMetricsQuery** + handler — aggregates counts from NAS + session repositories, logs `NasStatusChanged` when toggling
- **GetSessionsQuery** + handler — paginated session list with filters
- **GetAaaLogsQuery** + handler — paginated audit log with filters

#### New DTOs
- `AaaMetricsDto` — TotalNas, ActiveNas, InactiveNas, ActiveSessions, SessionsToday, TotalInputOctets, TotalOutputOctets
- `AaaAuditLogDto` — Id, TenantId, EventType, Username, NasId, NasIpAddress, Detail, Timestamp

#### Endpoint Registration
- `MetricsEndpoints.cs`: `group.MapGet("/metrics", ...)`
- Extended `NasEndpoints.cs`: add PUT + DELETE routes
- Extended `SessionEndpoints.cs`: add paginated list route
- `AuditLogEndpoints.cs`: `group.MapGet("/logs", ...)`

#### Migration
- New EF migration for `AaaAuditLog` table in `obss_aaa` schema

## Frontend Pages

All pages are `"use client"`, follow established patterns: React Query hooks, `DataTable`, shared components, `react-hook-form` + `zod`.

### Route Structure

```
/aaa                              → Dashboard
/aaa/nas                          → NAS device list
/aaa/nas/new                      → Create NAS
/aaa/nas/[id]                     → NAS detail
/aaa/nas/[id]/edit                → Edit NAS
/aaa/sessions                     → Session list (active + history)
/aaa/sessions/[id]                → Session detail
/aaa/logs                         → Audit log viewer
```

### Page Details

#### Dashboard (`/aaa`)
- 6 `MetricCard`s: Total NAS, Active NAS, Inactive NAS, Active Sessions, Sessions Today, Data Transferred (sum in/out)
- Recent activity feed: last 10 audit log entries as a compact table
- Data source: `GET /api/v1/aaa/metrics` + `GET /api/v1/aaa/logs?page=1&pageSize=10`

#### NAS List (`/aaa/nas`)
- `PageHeader` with title "NAS Devices" + "Register NAS" button
- `SearchBar` + `FilterBar` (NasType dropdown, Status dropdown)
- `DataTable` columns: Name, IP Address, Type, Status, Location, Created
- Row click → detail page
- Inline delete with confirmation dialog
- Data source: `GET /api/v1/aaa/nas` (paginated)

#### Create NAS (`/aaa/nas/new`)
- `FormPageLayout` with back link to `/aaa/nas`
- `FormSection` "Device Details": Name, IP Address (text), Secret (password field, masked), Type (select: BRAS/BNG/WLC/VSAT/UAG), Location (optional text)
- `FormActions` with back + submit
- Validation: name required, IP required (IP format), secret required (min 8 chars), type required
- On success: `toast("NAS registered")` + redirect to `/aaa/nas/[id]`

#### NAS Detail (`/aaa/nas/[id]`)
- `EntityHeader` with name, status badge, edit + delete actions
- `EntityTabs`:
  - **Overview**: `EntityMetadata` showing all fields (Name, IP, Type, Status, Location, Created, Updated)
  - **Sessions**: embedded `DataTable` of sessions for this NAS (filtered `GET /sessions?nasId=...`)
  - **Audit Log**: embedded `DataTable` of audit entries for this NAS
- Delete button opens confirmation dialog

#### Edit NAS (`/aaa/nas/[id]/edit`)
- Same form as Create, pre-filled from `GET /nas/{id}`
- On success: `toast("NAS updated")` + redirect to detail page

#### Session List (`/aaa/sessions`)
- `PageHeader` with title "RADIUS Sessions"
- `SearchBar` + `FilterBar` (Status: Active/Stopped/Interim, NAS select, username, date range)
- Default filter: status = Active (toggle to show all)
- `DataTable` columns: Session ID, Username, NAS Name, Framed IP, Status, Duration, Data (tx/rx), Started
- Row click → session detail
- Data source: `GET /api/v1/aaa/sessions` (paginated)

#### Session Detail (`/aaa/sessions/[id]`)
- `EntityHeader` with session ID, status badge
- `EntityMetadata` showing: Session ID, Username, NAS, Framed IP, Called Station, Calling Station, Status, Duration, Input Octets, Output Octets, Started At, Stopped At, Last Updated
- Related audit log entries section below

#### Audit Logs (`/aaa/logs`)
- `PageHeader` with title "Audit Logs"
- `SearchBar` + `FilterBar` (Event Type dropdown, username, NAS, date range)
- `DataTable` columns: Timestamp, Event Type, Username, NAS IP, Detail (truncated)
- Event types displayed as `StatusBadge` with color coding (green for success/start, red for failure/stop, blue for interim, gray for NAS events)
- Data source: `GET /api/v1/aaa/logs` (paginated)

### Query Hooks (in `src/api/hooks/`)

| File | Hook | Endpoint |
|------|------|----------|
| `useAaaMetrics.ts` | `useAaaMetrics()` | `GET /aaa/metrics` |
| `useNasDevices.ts` | `useNasDevices(filters)` | `GET /aaa/nas` |
| `useNasDevice.ts` | `useNasDevice(id)` | `GET /aaa/nas/{id}` |
| `useCreateNas.ts` | `useCreateNas()` | `POST /aaa/nas` |
| `useUpdateNas.ts` | `useUpdateNas()` | `PUT /aaa/nas/{id}` |
| `useDeleteNas.ts` | `useDeleteNas()` | `DELETE /aaa/nas/{id}` |
| `useUpdateNasStatus.ts` | `useUpdateNasStatus()` | `PATCH /aaa/nas/{id}/status` |
| `useSessions.ts` | `useSessions(filters)` | `GET /aaa/sessions` |
| `useSession.ts` | `useSession(id)` | `GET /aaa/sessions/{id}` |
| `useAaaLogs.ts` | `useAaaLogs(filters)` | `GET /aaa/logs` |

### Query Keys (`src/lib/query-keys.ts`)

```typescript
aaa: {
  metrics: () => ["aaa", "metrics"],
  nas: {
    all: () => ["aaa", "nas"],
    list: (filters) => ["aaa", "nas", "list", filters],
    detail: (id) => ["aaa", "nas", "detail", id],
  },
  sessions: {
    all: () => ["aaa", "sessions"],
    list: (filters) => ["aaa", "sessions", "list", filters],
    detail: (id) => ["aaa", "sessions", "detail", id],
  },
  logs: {
    list: (filters) => ["aaa", "logs", "list", filters],
  },
}
```

### Sidebar

Add expandable "AAA" sub-menu in `ModuleSidebar.tsx` between Tickets and Network:

```
▶ AAA
   ├ Dashboard    (/aaa)
   ├ NAS Devices  (/aaa/nas)
   ├ Sessions     (/aaa/sessions)
   └ Audit Logs   (/aaa/logs)
```

Icon: `Shield` (from `lucide-react`, already imported in sidebar).

### Breadcrumb Labels

Add to `BreadcrumbBuilder.tsx` `labelMap`:
- `/aaa` → `"AAA"`
- `/aaa/nas` → `"NAS Devices"`
- `/aaa/sessions` → `"Sessions"`
- `/aaa/logs` → `"Audit Logs"`

## Files to Create/Modify

### Backend (C#)
- `src/Modules/AAA/Obss.AAA.Domain/Entities/AaaAuditLog.cs` — new entity
- `src/Modules/AAA/Obss.AAA.Domain/ValueObjects/AaaEventType.cs` — new enum
- `src/Modules/AAA/Obss.AAA.Application/Abstractions/IAaaAuditLogRepository.cs` — new
- `src/Modules/AAA/Obss.AAA.Application/DTOs/AaaMetricsDto.cs` — new
- `src/Modules/AAA/Obss.AAA.Application/DTOs/AaaAuditLogDto.cs` — new
- `src/Modules/AAA/Obss.AAA.Application/Commands/UpdateNas/UpdateNasCommand.cs` — new
- `src/Modules/AAA/Obss.AAA.Application/Commands/UpdateNas/UpdateNasCommandHandler.cs` — new
- `src/Modules/AAA/Obss.AAA.Application/Commands/UpdateNas/UpdateNasCommandValidator.cs` — new
- `src/Modules/AAA/Obss.AAA.Application/Commands/DeleteNas/DeleteNasCommand.cs` — new
- `src/Modules/AAA/Obss.AAA.Application/Commands/DeleteNas/DeleteNasCommandHandler.cs` — new
- `src/Modules/AAA/Obss.AAA.Application/Queries/GetAaaMetrics/GetAaaMetricsQuery.cs` — new
- `src/Modules/AAA/Obss.AAA.Application/Queries/GetAaaMetrics/GetAaaMetricsQueryHandler.cs` — new
- `src/Modules/AAA/Obss.AAA.Application/Queries/GetSessions/GetSessionsQuery.cs` — new
- `src/Modules/AAA/Obss.AAA.Application/Queries/GetSessions/GetSessionsQueryHandler.cs` — new
- `src/Modules/AAA/Obss.AAA.Application/Queries/GetAaaLogs/GetAaaLogsQuery.cs` — new
- `src/Modules/AAA/Obss.AAA.Application/Queries/GetAaaLogs/GetAaaLogsQueryHandler.cs` — new
- `src/Modules/AAA/Obss.AAA.Infrastructure/Persistence/Repositories/AaaAuditLogRepository.cs` — new
- `src/Modules/AAA/Obss.AAA.Infrastructure/Persistence/Configurations/AaaAuditLogConfiguration.cs` — new EF config
- `src/Modules/AAA/Obss.AAA.Infrastructure/EventHandlers/LogSessionStartedHandler.cs` — new domain event handler
- `src/Modules/AAA/Obss.AAA.Infrastructure/EventHandlers/LogSessionStoppedHandler.cs` — new
- `src/Modules/AAA/Obss.AAA.Api/Endpoints/MetricsEndpoints.cs` — new
- `src/Modules/AAA/Obss.AAA.Api/Endpoints/AuditLogEndpoints.cs` — new
- Modify `NasEndpoints.cs` — add PUT + DELETE
- Modify `SessionEndpoints.cs` — add paginated GET
- Modify `RegisterNasCommandHandler.cs` — add audit log entry
- Modify `UpdateNasStatusCommandHandler.cs` — add audit log entry
- Modify DI registration in infrastructure module

### Frontend (TypeScript/React)
- `src/api/hooks/useAaaMetrics.ts` — new
- `src/api/hooks/useNasDevices.ts` — new
- `src/api/hooks/useNasDevice.ts` — new
- `src/api/hooks/useCreateNas.ts` — new
- `src/api/hooks/useUpdateNas.ts` — new
- `src/api/hooks/useDeleteNas.ts` — new
- `src/api/hooks/useUpdateNasStatus.ts` — new
- `src/api/hooks/useSessions.ts` — new
- `src/api/hooks/useSession.ts` — new
- `src/api/hooks/useAaaLogs.ts` — new
- `src/api/generated/dto.ts` — add AaaMetricsDto, AaaAuditLogDto types
- `src/app/aaa/page.tsx` — Dashboard
- `src/app/aaa/nas/page.tsx` — NAS list
- `src/app/aaa/nas/new/page.tsx` — Create NAS
- `src/app/aaa/nas/[id]/page.tsx` — NAS detail
- `src/app/aaa/nas/[id]/edit/page.tsx` — Edit NAS
- `src/app/aaa/sessions/page.tsx` — Session list
- `src/app/aaa/sessions/[id]/page.tsx` — Session detail
- `src/app/aaa/logs/page.tsx` — Audit logs
- `src/lib/query-keys.ts` — add `aaa` query keys
- `src/components/shared/ModuleSidebar.tsx` — add AAA expandable sub-menu
- `src/components/shared/BreadcrumbBuilder.tsx` — add AAA label mappings

## Testing

- New unit tests for `UpdateNasCommandHandler`, `DeleteNasCommandHandler`, `GetAaaMetricsQueryHandler`, `GetAaaLogsQueryHandler`
- New integration test for `AaaAuditLogRepository`
- Verify all pages render and filters work via frontend type-check

## Out of Scope

- Real-time session updates (WebSocket/polling) — sessions reflect DB state, manual refresh
- Export (CSV/PDF) for logs — could be added later via the Reporting module
- Bulk NAS operations — defer to later
