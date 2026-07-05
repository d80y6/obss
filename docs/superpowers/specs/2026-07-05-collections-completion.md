# Collections (TMF 658) — Full Frontend & Backend Completion

## Architecture
Backend-first, then frontend. Follow existing OBSS patterns: CQRS via MediatR, Result<T>, FluentValidation, Mapster, EF Core, Minimal API endpoints.

## Backend Changes

### New Endpoints (under `/api/v1/collections`)
| Method | Route | Command/Query |
|--------|-------|---------------|
| GET | `/dunning-policies` | `GetDunningPoliciesQuery` (optional `?activeOnly=true`) |
| GET | `/dunning-policies/{id}` | `GetDunningPolicyByIdQuery` |
| POST | `/dunning-policies` | `CreateDunningPolicyCommand` |
| PUT | `/dunning-policies/{id}` | `UpdateDunningPolicyCommand` |
| DELETE | `/dunning-policies/{id}` | `DeleteDunningPolicyCommand` |

Each command → 3 files (record, handler, validator). Each query → 2 files (record, handler).
New DTO: `DunningPolicyDto`.

### Fixes
1. **Pagination** — `GetCollectionCasesQuery` handler: add `.Skip().Take()` via `FindAsync` override.
2. **Aging report** — populate `CustomerCount` and `TotalCustomers` via distinct customer grouping.
3. **DunningFees persistence** — replace `builder.Ignore()` with `HasColumnType("jsonb")` for `Dictionary<int, decimal>`.
4. **Type exports** — add `DunningPolicyDto`, `AddCollectionActionCommand`, `RecordArrangementPaymentCommand` to `index.ts`.

## Frontend Changes

### New Pages
| Route | Description |
|-------|-------------|
| `/collections/dunning-policies` | DataTable list |
| `/collections/dunning-policies/[id]` | Detail view with fee table |
| `/collections/dunning-policies/new` | Create form |
| `/collections/dunning-policies/[id]/edit` | Edit form |
| `/collections/dashboard` | 4 stat cards + recent activity |

### UI Enhancements
- **Case detail**: Installment table per arrangement, dunning level badge, record payment dialog
- **Aging report**: Add bar chart visualization

### New Hooks
`useDunningPolicies`, `useDunningPolicy`, `useCreateDunningPolicy`, `useUpdateDunningPolicy`, `useDeleteDunningPolicy`, `useRecordArrangementPayment`, `useCollectionDashboard`

### New Query Keys
`dunningPolicies` (all, lists, detail/id), `dashboard`
