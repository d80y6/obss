# Team D: Support & Cross-Cutting Implementation Plan

> **For agentic workers:** Build/fix pages for Ticketing, Notifications, Reporting, Audit, API Gateway. These modules have list pages but are missing detail pages, create/edit forms, search, filters, pagination.

**Goal:** Complete ticketing (edit, assignment, SLA, comments form, escalation), notifications (detail, templates CRUD, preferences), reporting (report builder, execute, export, scheduled CRUD), audit (entry detail, alert rules, export), API gateway (route/API key/partner CRUD)

**Architecture:** Next.js App Router + TypeScript + shadcn UI. Follow existing patterns in `src/app/tickets/`, `src/app/audit/`, `src/app/reporting/`.

---

### Task 1: Ticketing — Edit + Assignment + Status Transitions

**Files:**
- Create: `src/app/tickets/[id]/edit/page.tsx`
- Modify: `src/app/tickets/[id]/page.tsx` (add assign, resolve, close, escalate)

- [ ] **Create edit form** — title, description, category, priority, assignedTo (user select)
- [ ] **Add action buttons** — Assign (POST .../assign with user select), Resolve (.../resolve), Close (.../close), Escalate (.../escalate with reason)
- [ ] **Add SLA status** — time remaining, breached indicator

### Task 2: Ticketing — Comments CRUD

- [ ] **Add comment form** to ticket detail — textarea, submit to `POST /api/v1/ticketing/tickets/{id}/comments`
- [ ] **Display comment thread** — author, timestamp, content, internal/external badge
- [ ] **Add SLA definitions page** if not existing — list, create form (name, priority, responseTime, resolutionTime)

### Task 3: Ticketing — SLA Views + Escalations

**Files:**
- Create: `src/app/tickets/sla-breached/page.tsx`
- Create: `src/api/hooks/use-sla-definitions.ts`

- [ ] **Create SLA breached list** — tickets past SLA thresholds with response/resolution times
- [ ] **Add escalation view** — escalated tickets list with escalation reason, date, assigned agent

### Task 4: Notifications — Detail + Templates CRUD + Preferences

**Files:**
- Create: `src/app/notifications/[id]/page.tsx`
- Create: `src/app/notifications/templates/new/page.tsx`
- Create: `src/app/notifications/templates/[id]/page.tsx`
- Create: `src/app/notifications/templates/[id]/edit/page.tsx`
- Create: `src/app/notifications/preferences/page.tsx`
- Create: `src/api/hooks/use-notifications.ts`
- Create: `src/api/hooks/use-notification-templates.ts`
- Create: `src/api/hooks/use-notification-preferences.ts`

- [ ] **Create API hooks** for notifications, templates, preferences
- [ ] **Add SearchBar + FilterBar** (type, status, channel) to notifications list
- [ ] **Create notification detail page** — type, channel, recipient, subject, body, status, sentAt, readAt
- [ ] **Add "Mark as Read" action** — POST to mark
- [ ] **Create template list** — with create/edit/delete
- [ ] **Create template form** — name, type (email/SMS/push), subject, body (with variable placeholders), channel
- [ ] **Create preferences page** — per channel (email/SMS/push) toggle on/off, notification types

### Task 5: Reporting — Report Builder

**Files:**
- Create: `src/app/reporting/definitions/new/page.tsx`
- Create: `src/app/reporting/definitions/[id]/page.tsx`
- Create: `src/app/reporting/definitions/[id]/edit/page.tsx`
- Create: `src/api/hooks/use-reporting.ts`

- [ ] **Create API hooks** for report definitions, executions, scheduled reports
- [ ] **Create report definition form** — name, description, category, parameters (name, type, default), query/config
- [ ] **Create definition detail page** — name, parameters, last executed, schedule, Audit tab
- [ ] **Add "Execute Now" button** — POST to `/api/v1/reporting/definitions/{id}/execute`

### Task 6: Reporting — Execute + Export + Schedule

**Files:**
- Create: `src/app/reporting/executions/[id]/page.tsx`
- Create: `src/app/reporting/scheduled/new/page.tsx`
- Create: `src/app/reporting/scheduled/[id]/page.tsx`
- Modify: `src/app/reporting/definitions/[id]/page.tsx` (add executions table)

- [ ] **Create execution detail page** — status, progress, startedAt, completedAt, result preview, download links
- [ ] **Add export buttons** — PDF, CSV, Excel download links
- [ ] **Create scheduled report form** — definition select, cron/frequency, recipients, format
- [ ] **Create scheduled report detail page** — definition, schedule, last run, next run, status, Audit tab
- [ ] **Add executions history table** to definition detail

### Task 7: Reporting — Dashboard Customization

**Files:**
- Modify: `src/app/reporting/page.tsx` (add widget management)

- [ ] **Make dashboard widgets configurable** — add/remove widgets, resize, drag reorder
- [ ] **Add "Add Widget" form** — type (metric, chart, table, list), title, reportDefinitionId, params
- [ ] **Add refresh interval** per widget

### Task 8: Audit — Entry Detail + Filters + Export

**Files:**
- Create: `src/app/audit/[id]/page.tsx`
- Modify: `src/app/audit/page.tsx` (add filters, pagination, export)
- Create: `src/api/hooks/use-audit-entries.ts`
- Create: `src/api/hooks/use-audit-alerts.ts`

- [ ] **Create API hooks** for audit entries, alerts
- [ ] **Add FilterBar** — entity type, actor, action, date range
- [ ] **Add SearchBar** — search entity ID, description
- [ ] **Add Pagination** to entries and alerts lists
- [ ] **Create entry detail page** — entity type, entity ID, action, actor, timestamp, changes (before/after diff), IP, metadata
- [ ] **Add export button** — CSV download

### Task 9: Audit — Alert Rules CRUD

**Files:**
- Create: `src/app/audit/alert-rules/page.tsx`
- Create: `src/app/audit/alert-rules/new/page.tsx`
- Create: `src/app/audit/alert-rules/[id]/page.tsx`
- Create: `src/app/audit/alert-rules/[id]/edit/page.tsx`

- [ ] **Create alert rules list** with DataTable (name, event type, severity, enabled)
- [ ] **Create alert rule form** — name, event type, severity (info/warning/critical), threshold, enabled
- [ ] **Create alert rule detail page** — rule config, recent matching alerts, Audit tab
- [ ] **Add acknowledge action** to alerts list

### Task 10: API Gateway — Routes CRUD

**Files:**
- Create: `src/app/api-gateway/routes/new/page.tsx`
- Create: `src/app/api-gateway/routes/[id]/page.tsx`
- Create: `src/app/api-gateway/routes/[id]/edit/page.tsx`
- Create: `src/api/hooks/use-api-gateway.ts`

- [ ] **Create API hooks** for routes, API keys, partners
- [ ] **Add SearchBar + FilterBar** to routes list
- [ ] **Create route form** — path, method (GET/POST/PUT/DELETE), upstream URL, auth required, rate limit
- [ ] **Create route detail page** — path, method, upstream, auth config, rate limit, status, Audit tab

### Task 11: API Gateway — API Keys CRUD

**Files:**
- Create: `src/app/api-gateway/api-keys/new/page.tsx`
- Create: `src/app/api-gateway/api-keys/[id]/page.tsx`

- [ ] **Create API key form** — name, partner select, expiry date, permissions/scopes
- [ ] **Create API key detail page** — name, key (masked), partner, created, expiry, last used, status, Revoke button, Audit tab

### Task 12: API Gateway — Partners CRUD

**Files:**
- Create: `src/app/api-gateway/partners/new/page.tsx`
- Create: `src/app/api-gateway/partners/[id]/page.tsx`
- Create: `src/app/api-gateway/partners/[id]/edit/page.tsx`

- [ ] **Create partner form** — name, contact email, contact phone, status
- [ ] **Create partner detail page** — details, API keys list, active routes, Audit tab
- [ ] **Wire partners list** — add create button, clickable rows
