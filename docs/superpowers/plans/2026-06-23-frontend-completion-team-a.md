# Team A: Core Business Flow Implementation Plan

> **For agentic workers:** Build/fix pages for IAM, CRM, Products, Orders, Subscriptions following existing patterns in the codebase. All API hooks exist. All backend endpoints exist.

**Goal:** Complete IAM (tenant mgmt), CRM (segments, contacts, notes CRUD), Products (offer/category CRUD, pricing), Orders (wizard, tracking, search), Subscriptions (create, lifecycle actions)

**Architecture:** Next.js App Router + TypeScript + shadcn UI. Follow existing patterns in `src/app/customers/`, `src/app/admin/`, `src/app/products/`.

**Patterns to follow:**
- List pages: use `DataTable` from `@/components/shared/data-table` with columns, search via `SearchBar`, filters via `FilterBar`
- Detail pages: use `EntityHeader`, `EntityTabs`, `EntityMetadata` from `@/components/shared/`
- Forms: use `FormPageLayout`, `FormSection`, `FormField`, `FormActions`, `FormErrorSummary` from `@/forms/`
- API hooks: in `src/api/hooks/` follow pattern of `useCustomers`, `useCreateCustomer`, etc.
- Audit timeline: use `/api/v1/audit/entities/{EntityType}/{id}` endpoint

---

### Task 1: CRM — Segments CRUD

**Files:**
- Create: `src/app/customers/segments/page.tsx`
- Create: `src/app/customers/segments/new/page.tsx`
- Create: `src/app/customers/segments/[id]/page.tsx`
- Create: `src/app/customers/segments/[id]/edit/page.tsx`
- Create: `src/api/hooks/use-segments.ts`

- [ ] **Create API hook** `useSegments` (list), `useSegment` (detail), `useCreateSegment`, `useUpdateSegment` — endpoints: `GET /api/v1/crm/segments`, `GET /api/v1/crm/segments/{id}`, `POST /api/v1/crm/segments`, `POST /api/v1/crm/segments/{id}/assign/{customerId}`, `DELETE /api/v1/crm/segments/{id}/customers/{customerId}`
- [ ] **Create segments list page** with DataTable (name, description, customer count, created date), SearchBar, EmptyState
- [ ] **Create segment detail page** with tabs: Overview (name, description, rules), Assigned Customers (list with remove action), Audit
- [ ] **Create segment form** (create + edit) with name, description, rules (JSON)
- [ ] **Add segment link** to customers detail page's Segments tab

### Task 2: CRM — Contact CRUD Forms

**Files:**
- Modify: `src/app/customers/[id]/page.tsx` (contacts tab)
- Create: `src/app/customers/[id]/contacts/new/page.tsx`
- Create: `src/app/customers/[id]/contacts/[contactId]/edit/page.tsx`

- [ ] **Add create contact button and form** — name, email, phone, role, isPrimary. POST to `/api/v1/crm/customers/{id}/contacts`
- [ ] **Add edit contact form** — same fields, inline edit
- [ ] **Add contact list** in the contacts tab with name, email, phone, role, primary badge, edit/delete actions

### Task 3: CRM — Note CRUD Forms

**Files:**
- Modify: `src/app/customers/[id]/page.tsx` (notes tab)
- [ ] **Add note create form** — textarea content, category select, submit to `POST /api/v1/crm/customers/{id}/notes`
- [ ] **Display notes list** with content, category badge, author, timestamp, delete action

### Task 4: Products — Offer CRUD

**Files:**
- Create: `src/app/products/offers/[id]/page.tsx`
- Create: `src/app/products/offers/new/page.tsx`
- Create: `src/app/products/offers/[id]/edit/page.tsx`
- Modify: `src/app/products/offers/page.tsx` (add create/edit links)

- [ ] **Create offer detail page** with tabs: Overview (name, description, product, pricing), Audit
- [ ] **Create offer form** — name, description, productId (select from products), validFrom, validTo, pricing table (recurring/one-time, amount, currency)
- [ ] **Wire offers list** — make product names clickable, add create button, add row actions

### Task 5: Products — Category CRUD

**Files:**
- Create: `src/app/products/categories/new/page.tsx`
- Create: `src/app/products/categories/[id]/page.tsx`
- Create: `src/app/products/categories/[id]/edit/page.tsx`
- Modify: `src/app/products/categories/page.tsx` (add create/edit links)

- [ ] **Create category detail page** — name, description, parent category, product count, Audit tab
- [ ] **Create category form** — name, description, parentCategoryId (tree select)
- [ ] **Wire categories list** — add create button, clickable rows

### Task 6: Orders — Search + Wizard + Tracking

**Files:**
- Modify: `src/app/orders/page.tsx` (add SearchBar)
- Modify: `src/app/orders/new/page.tsx` (multi-step wizard)
- Modify: `src/app/orders/[id]/page.tsx` (add tracking timeline)
- Create: `src/app/orders/[id]/tracking/page.tsx`

- [ ] **Add SearchBar** to orders list page, wire to `search` query param
- [ ] **Convert create form to wizard** — Step 1: select customer, Step 2: add items (product select + quantity), Step 3: review + submit
- [ ] **Add tracking timeline** — fetch from `GET /api/v1/orders/orders/{id}/fulfillment`, display as stepper
- [ ] **Add cancel button** — POST to `/api/v1/orders/orders/{id}/cancel`

### Task 7: Subscriptions — Create + Lifecycle

**Files:**
- Create: `src/app/subscriptions/new/page.tsx`
- Modify: `src/app/subscriptions/[id]/page.tsx` (add lifecycle actions)
- Modify: `src/app/subscriptions/[id]/edit/page.tsx` (add offer change, add-ons)

- [ ] **Create subscription form** — select customer, select offer, quantity, startDate
- [ ] **Add lifecycle action buttons** — Activate (`POST .../activate`), Suspend (`.../suspend`), Resume (`.../resume`), Cancel (`.../cancel`) with confirmation dialogs
- [ ] **Add add-ons section** to detail page — list of add-on entitlements with add/remove
- [ ] **Add offer change** — select from available offers, confirm dialog

### Task 8: IAM — Tenant Management

**Files:**
- Create: `src/app/admin/tenants/page.tsx`
- Create: `src/app/admin/tenants/new/page.tsx`
- Create: `src/app/admin/tenants/[id]/page.tsx`
- Create: `src/api/hooks/use-tenants.ts`

- [ ] **Create API hook** `useTenants`, `useCreateTenant` — `GET /api/v1/iam/tenants`, `POST /api/v1/iam/tenants`
- [ ] **Create tenants list page** with DataTable, search
- [ ] **Create tenant detail page** — name, slug, settings, Audit tab
- [ ] **Create tenant form** — name, slug, description
