# Team C: Infrastructure & Operations Implementation Plan

> **For agentic workers:** Build/fix pages for Service Inventory (completely empty), Network, Provisioning, Workflow. These modules have list+detail pages but lack create/edit forms, search, filters, pagination.

**Goal:** Complete service inventory (full module buildout from scratch), network (OLT detail, VLAN detail, PON ports, create/edit, topology), provisioning (job create/trigger, templates CRUD), workflow (definition designer, instance control, SLA)

**Architecture:** Next.js App Router + TypeScript + shadcn UI. Follow patterns in `src/app/customers/`, `src/app/network/`, `src/app/provisioning/`.

---

### Task 1: Service Inventory — Full Module (from scratch)

**Files:**
- Create: `src/app/service-inventory/page.tsx`
- Create: `src/app/service-inventory/[id]/page.tsx`
- Create: `src/app/service-inventory/new/page.tsx`
- Create: `src/app/service-inventory/[id]/edit/page.tsx`
- Create: `src/api/hooks/use-services.ts`
- Create: `src/api/hooks/use-service-topology.ts`

- [ ] **Create API hook** `useServices` (list), `useService` (detail), `useCreateService`, `useActivateService`, `useSuspendService`, `useDecommissionService`
- [ ] **Create service list page** with DataTable (name, type, status, customer, subscription), SearchBar, FilterBar (status, type), Pagination, BulkActions
- [ ] **Create service detail page** with tabs: Overview (name, type, status, customer, subscription, dates, attributes), Topology (upstream/downstream services), Resources, Audit
- [ ] **Create service form** — name, type (select), customerId, subscriptionId, attributes (key-value pairs)
- [ ] **Add action buttons** — Activate (POST .../activate), Suspend (.../suspend), Decommission (.../decommission)
- [ ] **Add topology visualization** — list of upstream/downstream services with link types

### Task 2: Service Inventory — Resources + Discovery

**Files:**
- Create: `src/app/service-inventory/[id]/resources/page.tsx`
- Create: `src/app/service-inventory/discovery/page.tsx`

- [ ] **Create resources tab** — table of resources linked to the service (name, type, status, network element)
- [ ] **Create discovery jobs page** — list of jobs, trigger new discovery, view unmatched resources

### Task 3: Network — OLT Detail + PON Ports

**Files:**
- Create: `src/app/network/olts/[id]/page.tsx`
- Create: `src/api/hooks/use-olts.ts`

- [ ] **Create API hook** `useOlts`, `useOlt` — `GET /api/v1/network/olts`
- [ ] **Create OLT detail page** — name, vendor, model, location, status, PON ports table (port index, status, ont count), Audit tab
- [ ] **Add "Create OLT" button** to OLTs list

### Task 4: Network — VLAN Detail + Create

**Files:**
- Create: `src/app/network/vlans/[id]/page.tsx`
- Create: `src/app/network/vlans/new/page.tsx`
- Create: `src/api/hooks/use-vlans.ts`

- [ ] **Create API hook** `useVlans`, `useVlan`, `useCreateVlan`
- [ ] **Create VLAN detail page** — vlanId, name, description, status, segments, Audit tab
- [ ] **Create VLAN form** — vlanId (number), name, description, segment

### Task 5: Network — Element Create/Edit

**Files:**
- Create: `src/app/network/elements/new/page.tsx`
- Create: `src/app/network/elements/[id]/edit/page.tsx`

- [ ] **Create element form** — name, type (OLT/ONT/switch/router/firewall), vendor, model, ipAddress, location, status
- [ ] **Create edit form** — same fields, pre-populated

### Task 6: Network — Topology + Capacity

**Files:**
- Create: `src/app/network/topology/page.tsx`
- Modify: `src/app/network/elements/[id]/page.tsx` (add connections, capacity)

- [ ] **Create topology page** — list of links (source, target, type, status), create link form
- [ ] **Add connections table** to element detail — connected elements, link type, status
- [ ] **Add capacity section** to element detail — capacity records, alerts, utilization chart

### Task 7: Provisioning — Job Create/Trigger + Logs

**Files:**
- Create: `src/app/provisioning/jobs/new/page.tsx`
- Create: `src/app/provisioning/jobs/[id]/logs/page.tsx`
- Create: `src/api/hooks/use-provisioning-jobs.ts`
- Create: `src/api/hooks/use-provisioning-templates.ts`

- [ ] **Create API hooks** for provisioning jobs and templates
- [ ] **Add SearchBar + FilterBar** (status, type) to jobs list
- [ ] **Create job form** — template select, target (service/device), parameters (key-value)
- [ ] **Add action buttons** to job detail — Start (POST .../start), Retry
- [ ] **Create logs tab** — step-by-step execution log, results, error details

### Task 8: Provisioning — Templates CRUD

**Files:**
- Create: `src/app/provisioning/templates/new/page.tsx`
- Create: `src/app/provisioning/templates/[id]/page.tsx`
- Create: `src/app/provisioning/templates/[id]/edit/page.tsx`

- [ ] **Create template form** — name, type, description, parameters (name, type, default, required), steps
- [ ] **Create template detail page** — name, type, parameter list, step list, Audit tab
- [ ] **Wire templates list** — add create button, clickable rows

### Task 9: Workflow — Definition Designer

**Files:**
- Create: `src/app/workflow/definitions/new/page.tsx`
- Create: `src/app/workflow/definitions/[id]/edit/page.tsx`
- Modify: `src/app/workflow/definitions/[id]/page.tsx` (add step ordering)

- [ ] **Create definition form** — name, description, category, version
- [ ] **Add step management** to definition detail — list steps in order, add step (name, type, config JSON), remove step, reorder
- [ ] **Wire definitions list** — add create button, clickable rows

### Task 10: Workflow — Instance Control + SLA

**Files:**
- Modify: `src/app/workflow/instances/[id]/page.tsx` (add execute/complete/fail buttons)
- Modify: `src/app/workflow/page.tsx` (add instance start)
- Create: `src/app/workflow/slas/page.tsx`
- Create: `src/app/workflow/slas/new/page.tsx`
- Create: `src/api/hooks/use-workflow-slas.ts`

- [ ] **Add "Start Instance" button** to definition detail — select definition, confirm
- [ ] **Add action buttons** to instance detail — Execute Task (for pending tasks), Complete, Fail with reason
- [ ] **Create SLA definitions list + form** — name, definition, warning threshold, critical threshold, escalation
- [ ] **Add SLA status** to instance detail — compliance %, breached tasks, timeline
