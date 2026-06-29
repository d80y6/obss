# Team B: Revenue Cycle Implementation Plan

> **For agentic workers:** Build/fix pages for Billing, Invoices, Payments, Collections. Most list+detail pages exist but lack create/edit forms, search, filters, and lifecycle actions.

**Goal:** Complete billing (cycles, jobs, adjustments, tax rules), invoices (create, send, PDF, disputes, credit notes), payments (record, refund, reconcile), collections (cases, actions, arrangements, dunning)

**Architecture:** Next.js App Router + TypeScript + shadcn UI. Follow existing patterns in `src/app/customers/`, `src/app/invoices/`, `src/app/billing/`.

---

### Task 1: Billing — API Hooks Standardization

- [ ] **Create dedicated hooks** in `src/api/hooks/use-billing-cycles.ts`, `use-billing-jobs.ts`, `use-tax-rules.ts` following existing patterns
- [ ] Replace inline `useQuery` calls in pages with new hooks

### Task 2: Billing — Cycles CRUD

**Files:**
- Create: `src/app/billing/cycles/new/page.tsx`
- Create: `src/app/billing/cycles/[id]/page.tsx`
- Modify: `src/app/billing/cycles/page.tsx` (add create/edit links, search)

- [ ] **Add SearchBar and filters** to cycles list
- [ ] **Create cycle detail page** — name, billing date, period, status, bill count, Audit tab
- [ ] **Create cycle form** — name, billingDay, frequency (monthly/quarterly/annual), periodStart/End
- [ ] **Add "Generate Cycle" button** — POST to `/api/v1/billing/cycles`

### Task 3: Billing — Jobs Management

**Files:**
- Create: `src/app/billing/jobs/[id]/page.tsx`
- Modify: `src/app/billing/jobs/page.tsx` (add search, trigger/cancel)

- [ ] **Add SearchBar and filters** to jobs list (status, type)
- [ ] **Create job detail page** — name, type, status, progress %, startedAt, completedAt, results, Audit tab
- [ ] **Add trigger/cancel buttons** — POST to trigger, POST to cancel
- [ ] **Add "New Job" button** — select type (ad-hoc billing run), form with params

### Task 4: Billing — Tax Rules

**Files:**
- Create: `src/app/billing/tax-rules/page.tsx`
- Create: `src/app/billing/tax-rules/new/page.tsx`

- [ ] **Create tax rules list page** with DataTable (name, rate, region, status)
- [ ] **Create tax rule form** — name, rate (%), region, product category, effective dates

### Task 5: Billing — Bill Adjustments

**Files:**
- Modify: `src/app/billing/[id]/page.tsx` (add adjustments section + actions)

- [ ] **Add adjustments table** to bill detail — amount, reason, createdBy, date
- [ ] **Add "Add Adjustment" form** — amount, reason, type (credit/debit)
- [ ] **Add "Finalize Bill" button** — POST to `/api/v1/billing/bills/{id}/finalize`
- [ ] **Add "Generate Invoice" button** — POST to `/api/v1/invoices/invoices` with billId

### Task 6: Invoices — Create + Send + PDF

**Files:**
- Modify: `src/app/invoices/page.tsx` (add SearchBar)
- Create: `src/app/invoices/new/page.tsx`
- Modify: `src/app/invoices/[id]/page.tsx` (add actions + PDF + payment allocation)

- [ ] **Add SearchBar** to invoices list — search by number, customer name
- [ ] **Create invoice form** — select bill (or customer + line items), due date, notes
- [ ] **Add action buttons** to detail page — Finalize (POST .../finalize), Send (POST .../send), Cancel (POST .../cancel)
- [ ] **Add PDF download** — link to `GET /api/v1/invoices/invoices/{id}/pdf`
- [ ] **Add payment allocation** — select from unallocated payments, partial amounts

### Task 7: Invoices — Credit Notes + Disputes Management

**Files:**
- Create: `src/app/invoices/[id]/credit-notes/new/page.tsx`
- Create: `src/app/invoices/[id]/disputes/[disputeId]/page.tsx`
- Modify: `src/app/invoices/credit-notes/page.tsx` (add create)
- Modify: `src/app/invoices/disputes/page.tsx` (add resolve/reject)

- [ ] **Add "Issue Credit Note" form** — reason, line items to credit, amount
- [ ] **Create dispute detail page** — reason, status, resolution, timeline, resolve/reject buttons
- [ ] **Wire dispute actions** — POST to resolve/reject endpoints

### Task 8: Payments — Record + Refund

**Files:**
- Create: `src/app/payments/new/page.tsx`
- Create: `src/app/payments/[id]/refund/page.tsx`
- Modify: `src/app/payments/page.tsx` (add SearchBar)

- [ ] **Add SearchBar** to payments list — search by reference, customer
- [ ] **Create payment form** — customer select, invoice select, amount, method (cash/check/card/bank), reference, date
- [ ] **Create refund form** — reason, amount (partial/full), method
- [ ] **Add action buttons** to detail page — Complete (POST .../complete), Refund

### Task 9: Payments — Reconciliation

**Files:**
- Modify: `src/app/payments/reconciliation/page.tsx` (add actions)

- [ ] **Add unmatched transactions table** — date, reference, amount, status
- [ ] **Add "Import Statement" button** — file upload for bank statements
- [ ] **Add "Auto-Reconcile" button** — POST to auto-reconcile
- [ ] **Add match action** — select payment to match with transaction

### Task 10: Collections — Cases + Actions + Arrangements

**Files:**
- Create: `src/app/collections/new/page.tsx`
- Modify: `src/app/collections/[id]/page.tsx` (add actions, arrangements)
- Create: `src/app/collections/[id]/arrangements/new/page.tsx`
- Create: `src/app/collections/reports/aging/page.tsx`

- [ ] **Add SearchBar and filters** (status, agent, aging bucket) to cases list
- [ ] **Create case form** — customer select, assigned agent, priority, notes
- [ ] **Add action buttons** to case detail — Add Action (reminder/call/letter/email), Send Dunning Notice, Resolve
- [ ] **Create payment arrangement form** — installment count, amount per installment, frequency, start date
- [ ] **Create aging report page** — buckets (0-30, 31-60, 61-90, 90+), totals per bucket
