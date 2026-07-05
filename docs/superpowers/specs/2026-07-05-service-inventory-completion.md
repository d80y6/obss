# Service Inventory (TMF 638) — Full Frontend & Backend Completion

## Architecture
Backend-first, then frontend. Fix critical bugs, add features in dependency order.

## Backend Changes
1. **TenantId** — Replace `Guid.Empty` with `_currentTenant.TenantId` in `CreateServiceCommandHandler`
2. **Pagination** — Add `x-total-count` response header in `GetServicesQueryHandler`
3. **Suspend reason** — Make `Reason` optional in `SuspendServiceCommandValidator`
4. **Provisioning lifecycle** — Call `SetProvisioning()` before `Activate()` in `ActivateServiceCommandHandler`

## Frontend Changes
1. **types.ts** — Add 3 missing endpoint signatures: PATCH /services/{id}, POST /services/{id}/resume, DELETE /services/topology/{topologyId}/links/{linkId}
2. **Sidebar** — Collapsible Service Inventory section with Services + Discovery sub-items
3. **List page** — Populate type filter with ServiceType enum values
4. **Topology visualization** — Detail page: add visual topology card with source→arrow→target links colored by type
5. **Unmatched resources** — New page at `/service-inventory/discovery/unmatched`
6. **Resource management** — Detail page Resources tab: add Release/Allocate actions
7. **Discovery workflow** — Sidebar link, auto-invalidate on start/complete, toasts
