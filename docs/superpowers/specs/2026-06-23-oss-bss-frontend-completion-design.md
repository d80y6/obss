# Frontend Completion Design: OSS/BSS Operator Portal

## Overview

Complete the remaining frontend pages for 18 modules in a Next.js OSS/BSS operator portal. Backend has 236 API endpoints across 19 modules. Frontend has 66 pages with varying completion states. Goal: every module has working list/detail/create/edit pages with search, filters, pagination, bulk actions, and audit timeline.

## Current State

- **Complete modules (IAM/CRM):** Full CRUD with search, filters, pagination, forms, audit
- **Partial modules (12):** List + detail pages exist but missing search/filter/pagination and create/edit forms
- **Missing modules (1):** service-inventory directory is completely empty
- **Underbuilt modules (5):** Notifications, Reporting, Audit, API Gateway — detail pages and forms missing

## Execution Plan

### Phase 1: Shared Infrastructure

Create missing API hooks for modules that currently use inline `useQuery`/`useMutation`:
- Collections, Services, Network, Provisioning, Workflow, Notifications, Reporting, Audit, API Gateway

Each hook follows the existing pattern: `useXxx` (list), `useXxx` (detail), `useCreateXxx`, `useUpdateXxx`

### Phase 2: Parallel Module Completion (4 Teams)

**Team A — Core Business Flow**
- IAM: Tenant management pages, permission display
- CRM: Segment pages (list/detail/create), contact CRUD forms, note CRUD forms
- Products: Offer CRUD (create/edit), Category CRUD, pricing configuration
- Orders: Order wizard flow, order tracking timeline, cancellation, search
- Subscriptions: Create page, activate/suspend/cancel actions, add-ons, offer change

**Team B — Revenue Cycle**
- Billing: Create billing cycles, billing job trigger/cancel, bill adjustments, tax rules
- Invoices: Create from bill, send/email, PDF download, payment allocation, dispute resolution, credit note create
- Payments: Record payment, refund initiation, reconciliation match/unmatch, gateway config
- Collections: Create case, actions (reminder/call), payment arrangement, aging/dunning

**Team C — Infrastructure & Operations**
- Service Inventory: Full module (list, detail, create, edit, topology visualization, activation/suspend)
- Network: OLT detail, VLAN detail, PON ports, element create/edit, topology, capacity
- Provisioning: Job create/trigger, template CRUD, step results, logs, retry
- Workflow: Definition designer, instance manual start, step detail, SLA tracking

**Team D — Support & Cross-Cutting**
- Ticketing: Edit page, assignment, status transitions, SLA views, comment form, escalation
- Notifications: Detail page, template CRUD, mark-as-read, preferences
- Reporting: Report builder, execute/run, export (PDF/CSV/Excel), scheduled CRUD
- Audit: Entry detail view, alert rule CRUD, export, filter by entity/actor/action
- API Gateway: Route detail/create/edit, API key CRUD/revoke, partner CRUD, rate limiting

### Phase 3: Cross-Cutting Verification

- Verify search/filter/pagination on all 18 modules
- Wire bulk actions (delete, status change, export)
- Verify audit timeline integration on all detail pages
- TypeScript build verification
- No mock data remains

## UI Standards

- Next.js App Router with TypeScript
- ShadCN UI + Radix primitives
- TanStack Query for server state
- React Hook Form + Zod for form validation
- RTL support (Arabic/English via locale provider)
- Dark mode via theme provider
- Responsive layout via app-shell
- Zustand for client state (auth, tenant, locale, theme)
- All API calls through shared Axios instance with auth interceptor

## Data Flow

```
Page → API Hook (TanStack Query) → Axios Client → Backend API
                                    ↓
                             Auth token + Tenant ID
                              (from stores)
```

## Verification

Each module must pass:
1. `npm run build` succeeds
2. No mock data or hardcoded values
3. All data tables wired to real APIs
4. Forms submit to real endpoints
5. Search/filter/pagination functional
6. TypeScript strict mode passes
7. All routes accessible with proper permissions
