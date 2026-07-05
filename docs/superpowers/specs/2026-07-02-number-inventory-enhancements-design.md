# NumberInventory Module Enhancements

## Objective
Add missing state transition endpoints and frontend coverage to the NumberInventory module to match the existing `TelephoneNumber` domain entity's full behavior.

## Backend

### New Commands (6)
| Command | Handler | Validator | Effect |
|---------|---------|-----------|--------|
| `ReserveNumberCommand(Guid NumberId)` | Loads number, calls `number.Reserve()`, saves | NumberId required | Available -> Reserved |
| `SuspendNumberCommand(Guid NumberId)` | Loads number, calls `number.Suspend()`, saves | NumberId required | Assigned/Ported -> Suspended |
| `ResumeNumberCommand(Guid NumberId)` | Loads number, calls `number.Resume()`, saves | NumberId required | Suspended -> Assigned |
| `DisconnectNumberCommand(Guid NumberId)` | Loads number, calls `number.Disconnect()`, saves | NumberId required | Assigned/Suspended/Ported -> Disconnected |
| `PortInNumberCommand(Guid NumberId, Guid? CustomerId)` | Loads number, calls `number.PortIn(CustomerId)`, saves | NumberId required | Available -> Ported |
| `PortOutNumberCommand(Guid NumberId)` | Loads number, calls `number.PortOut()`, saves | NumberId required | Assigned/Ported -> Available |

### New Endpoints (6)
All under `POST /api/v{version}/number-inventory/numbers/{id}/{action}`:
- `/reserve`, `/suspend`, `/resume`, `/disconnect`, `/port-in`, `/port-out`

### Domain Entity Changes
- Add `Resume()` method to `TelephoneNumber`. Transitions Suspended -> Assigned. CustomerId/SubscriptionId/AssignedAt are preserved from before suspension (Suspend does not clear them).

### Registration
- Commands auto-registered via MediatR assembly scan (no change needed)
- Validators auto-registered via FluentValidation assembly scan (no change needed)

## Frontend

### Sidebar
- Add "Number Inventory" nav item to `ModuleSidebar.tsx` (icon: `Phone`)

### New Hooks (6)
- `useReserveTelephoneNumber`
- `useSuspendTelephoneNumber`
- `useResumeTelephoneNumber`
- `useDisconnectTelephoneNumber`
- `usePortInTelephoneNumber`
- `usePortOutTelephoneNumber`

All follow the same pattern: mutationFn calls POST, onSuccess invalidates lists + detail.

### Detail Page Updates
- Add action buttons in the "Actions" tab conditioned on current status:
  - Available: Reserve, PortIn (experimental)
  - Reserved: Release (existing), PortIn
  - Assigned: Suspend, PortOut, Disconnect
  - Ported: Suspend, PortOut, Disconnect
  - Suspended: Resume, Disconnect
  - Disconnected: (no state transitions)

## Test Updates
- No new test files needed for this session. Existing 10 test files remain.

## Out of Scope
- Number ranges/blocks (new entity would require full design)
- Number reservations with expiry
- Bulk number import
