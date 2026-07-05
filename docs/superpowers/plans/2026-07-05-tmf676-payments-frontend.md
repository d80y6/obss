# TMF676 (Payments) Frontend Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development to implement this plan task-by-task.

**Goal:** Fill all frontend gaps in the Payments module: missing hooks, DTO alignment, inline API cleanup, gateway management pages, and payment summary.

**Architecture:** Follow existing hook patterns (api.post/get from `@/services/api`, queryKeys for invalidation, generated types from `@/api/generated`). Pages use `"use client"`, `DataTable`, entity patterns.

**Tech Stack:** Next.js 16 App Router, React 19, TanStack Query, Mapster-style object display, zod forms.

---

### Task 1: Fix DTOs and Add Missing Types

**Files:**
- Modify: `frontend/src/api/generated/dto.ts` (lines 800-838)
- Modify: `frontend/src/api/generated/index.ts`

- [ ] **Step 1: Update PaymentDto to match backend**

Change the PaymentDto (lines 823-838) to include backend fields:

```typescript
export interface PaymentDto {
  id: string;
  tenantId: string;
  paymentNumber: string;
  customerId: string;
  customerName: string;
  invoiceId: string | null;
  invoiceNumber: string | null;
  amount: number;
  currency: string;
  paymentMethod: string;
  paymentReference: string | null;
  status: string;
  paidAt: string;
  completedAt: string | null;
  notes: string | null;
  transactionId: string;
  createdAt: string;
  updatedAt: string;
  allocations: PaymentAllocationDto[];
  refunds: RefundDto[];
}
```

- [ ] **Step 2: Add missing DTOs**

Add before RefundDto:

```typescript
export interface PaymentAllocationDto {
  id: string;
  invoiceId: string;
  amount: number;
  createdAt: string;
}

export interface PaymentSummaryDto {
  totalPayments: number;
  pendingCount: number;
  completedCount: number;
  failedCount: number;
  refundedCount: number;
  partiallyRefundedCount: number;
  totalAmount: number;
  totalCompletedAmount: number;
  totalRefundedAmount: number;
  netAmount: number;
}

export interface PaymentGatewayDto {
  id: string;
  tenantId: string;
  name: string;
  provider: string;
  isActive: boolean;
  configuration: string;
  supportedCurrencies: string[];
  minAmount: number | null;
  maxAmount: number | null;
  transactionFee: number;
  feeType: string;
  createdAt: string;
}

export interface PaymentGatewayInfo {
  provider: string;
  displayName: string;
  isAvailable: boolean;
}
```

- [ ] **Step 3: Update ReconciliationDto and add ReconciliationItemDto**

Replace existing ReconciliationDto:

```typescript
export interface ReconciliationDto {
  id: string;
  tenantId: string;
  importDate: string;
  importSource: string;
  importFileName: string | null;
  status: string;
  totalImportAmount: number;
  totalReconciledAmount: number;
  currency: string;
  importedBy: string;
  createdAt: string;
  items: ReconciliationItemDto[];
}

export interface ReconciliationItemDto {
  id: string;
  reconciliationId: string;
  externalReference: string;
  amount: number;
  currency: string;
  transactionDate: string;
  description: string | null;
  matchedInvoiceId: string | null;
  matchedPaymentId: string | null;
  status: string;
  discrepancyReason: string | null;
  createdAt: string;
}
```

- [ ] **Step 4: Update index.ts exports**

Add to the re-export block:
```typescript
export type {
  ...
  PaymentAllocationDto, PaymentSummaryDto, PaymentGatewayDto, PaymentGatewayInfo,
  ReconciliationItemDto,
} from './dto';
```

- [ ] **Step 5: Update RefundDto to include completedAt**

```typescript
export interface RefundDto {
  id: string;
  refundNumber: string;
  paymentId: string;
  paymentNumber: string;
  amount: number;
  currency: string;
  reason: string;
  status: string;
  createdAt: string;
  completedAt: string | null;
}
```

- [ ] **Step 6: Add query keys for summary, gateways, by-invoice**

In `query-keys.ts`, after line 137 (`refunds` block), add:
```typescript
payments: {
  ...
  summary: ["payments", "summary"] as const,
  byInvoice: (invoiceId: string) => ["payments", "by-invoice", invoiceId] as const,
  gateways: {
    all: ["payments", "gateways"] as const,
    list: (filters: Record<string, string> = {}) =>
      [...queryKeys.payments.gateways.all, "list", filters] as const,
  },
  unmatched: ["payments", "unmatched"] as const,
  reconciliation: {
    all: ["payments", "reconciliation"] as const,
    list: (filters: Record<string, string> = {}) =>
      [...queryKeys.payments.reconciliation.all, "list", filters] as const,
    detail: (id: string) => [...queryKeys.payments.reconciliation.all, "detail", id] as const,
  },
  ...
}
```

### Task 2: Create Missing Hooks

**Files:**
- Create: `frontend/src/api/hooks/useCompletePayment.ts`
- Create: `frontend/src/api/hooks/useAllocatePayment.ts`
- Create: `frontend/src/api/hooks/usePaymentSummary.ts`
- Create: `frontend/src/api/hooks/usePaymentsByInvoice.ts`
- Create: `frontend/src/api/hooks/usePaymentGateways.ts`
- Create: `frontend/src/api/hooks/useCreatePaymentGateway.ts`
- Create: `frontend/src/api/hooks/useProcessGatewayPayment.ts`
- Create: `frontend/src/api/hooks/useReconciliationDetail.ts`
- Create: `frontend/src/api/hooks/useUnmatchedTransactions.ts`
- Create: `frontend/src/api/hooks/useImportBankStatement.ts`
- Create: `frontend/src/api/hooks/useAutoReconcile.ts`
- Create: `frontend/src/api/hooks/useMatchTransaction.ts`
- Modify: `frontend/src/api/hooks/useReconciliation.ts` (fix return type)
- Modify: `frontend/src/api/hooks/useRefunds.ts` (add filter support)
- Modify: `frontend/src/api/hooks/useCreatePayment.ts` (add summary invalidation)

- [ ] **Step 1: Create useCompletePayment**

```typescript
import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"

export function useCompletePayment(id: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async () => {
      const res = await api.post(`/api/v1/payments/payments/${id}/complete`)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.payments.detail(id) })
      queryClient.invalidateQueries({ queryKey: queryKeys.payments.all })
      queryClient.invalidateQueries({ queryKey: queryKeys.payments.summary })
    },
  })
}
```

- [ ] **Step 2: Create useAllocatePayment**

```typescript
import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { AllocatePaymentCommand } from "@/api/generated"

export function useAllocatePayment(paymentId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: AllocatePaymentCommand) => {
      const res = await api.post(`/api/v1/payments/payments/${paymentId}/allocate`, data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.payments.detail(paymentId) })
      queryClient.invalidateQueries({ queryKey: queryKeys.payments.all })
    },
  })
}
```

- [ ] **Step 3: Create usePaymentSummary**

```typescript
import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { PaymentSummaryDto } from '@/api/generated/dto'

export function usePaymentSummary() {
  return useQuery({
    queryKey: queryKeys.payments.summary,
    queryFn: async () => {
      const res = await api.get("/api/v1/payments/payments/summary")
      return res.data as PaymentSummaryDto
    },
  })
}
```

- [ ] **Step 4: Create usePaymentsByInvoice**

```typescript
import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { PaymentDto } from '@/api/generated/dto'

export function usePaymentsByInvoice(invoiceId: string) {
  return useQuery({
    queryKey: queryKeys.payments.byInvoice(invoiceId),
    queryFn: async () => {
      const res = await api.get(`/api/v1/payments/payments/by-invoice/${invoiceId}`)
      return res.data as PaymentDto[]
    },
    enabled: !!invoiceId,
  })
}
```

- [ ] **Step 5: Create usePaymentGateways**

```typescript
import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { PaymentGatewayInfo } from '@/api/generated/dto'

export function usePaymentGateways() {
  return useQuery({
    queryKey: queryKeys.payments.gateways.list({}),
    queryFn: async () => {
      const res = await api.get("/api/v1/payments/payments/gateways")
      return res.data as PaymentGatewayInfo[]
    },
  })
}
```

- [ ] **Step 6: Create useCreatePaymentGateway**

```typescript
import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { RegisterPaymentGatewayCommand, PaymentGatewayDto } from "@/api/generated"

export function useCreatePaymentGateway() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: RegisterPaymentGatewayCommand) => {
      const res = await api.post<PaymentGatewayDto>("/api/v1/payments/payments/gateways", data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.payments.gateways.all })
    },
  })
}
```

- [ ] **Step 7: Create useProcessGatewayPayment**

```typescript
import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { ProcessGatewayPaymentCommand, PaymentDto } from "@/api/generated"

export function useProcessGatewayPayment() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: ProcessGatewayPaymentCommand) => {
      const res = await api.post<PaymentDto>("/api/v1/payments/payments/process", data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.payments.all })
      queryClient.invalidateQueries({ queryKey: queryKeys.payments.summary })
    },
  })
}
```

- [ ] **Step 8: Create useReconciliationDetail**

```typescript
import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { ReconciliationDto } from '@/api/generated/dto'

export function useReconciliationDetail(id: string) {
  return useQuery({
    queryKey: queryKeys.payments.reconciliation.detail(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/payments/payments/reconciliation/${id}`)
      return res.data as ReconciliationDto
    },
    enabled: !!id,
  })
}
```

- [ ] **Step 9: Create useUnmatchedTransactions**

```typescript
import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { ReconciliationItemDto } from '@/api/generated/dto'

export function useUnmatchedTransactions() {
  return useQuery({
    queryKey: queryKeys.payments.unmatched,
    queryFn: async () => {
      const res = await api.get("/api/v1/payments/payments/reconciliation/unmatched")
      return res.data as ReconciliationItemDto[]
    },
  })
}
```

- [ ] **Step 10: Create useImportBankStatement**

```typescript
import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { ImportBankStatementCommand, PaymentReconciliationDto } from "@/api/generated"

export function useImportBankStatement() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: ImportBankStatementCommand) => {
      const res = await api.post<PaymentReconciliationDto>(
        "/api/v1/payments/payments/reconciliation/import", data
      )
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.payments.reconciliation.all })
      queryClient.invalidateQueries({ queryKey: queryKeys.payments.unmatched })
    },
  })
}
```

- [ ] **Step 11: Create useAutoReconcile**

```typescript
import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"

export function useAutoReconcile() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async () => {
      const res = await api.post("/api/v1/payments/payments/reconciliation/auto")
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.payments.reconciliation.all })
      queryClient.invalidateQueries({ queryKey: queryKeys.payments.unmatched })
    },
  })
}
```

- [ ] **Step 12: Create useMatchTransaction**

```typescript
import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"

export function useMatchTransaction() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ reconciliationId, itemId, matchedPaymentId }: {
      reconciliationId: string; itemId: string; matchedPaymentId: string
    }) => {
      const res = await api.post(
        `/api/v1/payments/payments/reconciliation/${reconciliationId}/match`,
        { itemId, matchedPaymentId }
      )
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.payments.reconciliation.all })
      queryClient.invalidateQueries({ queryKey: queryKeys.payments.unmatched })
    },
  })
}
```

- [ ] **Step 13: Fix useReconciliation to return array**

Replace content with:
```typescript
import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { ReconciliationDto } from '@/api/generated/dto'

export function useReconciliation(filters: Record<string, string> = {}) {
  return useQuery({
    queryKey: queryKeys.payments.reconciliation.list(filters),
    queryFn: async () => {
      const params = new URLSearchParams()
      Object.entries(filters).forEach(([k, v]) => { if (v) params.set(k, v) })
      const res = await api.get(`/api/v1/payments/payments/reconciliation?${params.toString()}`)
      return res.data as ReconciliationDto[]
    },
  })
}
```

- [ ] **Step 14: Fix useRefunds to support filters**

```typescript
import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { RefundDto } from '@/api/generated/dto'

export function useRefunds(filters: Record<string, string> = {}) {
  return useQuery({
    queryKey: queryKeys.payments.refunds.list(filters),
    queryFn: async () => {
      const params = new URLSearchParams()
      Object.entries(filters).forEach(([k, v]) => { if (v) params.set(k, v) })
      const res = await api.get(`/api/v1/payments/refunds?${params.toString()}`)
      return res.data as RefundDto[]
    },
  })
}
```

- [ ] **Step 15: Fix useCreatePayment to invalidate summary**

Add `queryClient.invalidateQueries({ queryKey: queryKeys.payments.summary })` to onSuccess.

### Task 3: Fix Existing Pages to Use Hooks

**Files:**
- Modify: `frontend/src/app/payments/[id]/page.tsx`
- Modify: `frontend/src/app/payments/reconciliation/page.tsx`
- Modify: `frontend/src/app/payments/refunds/page.tsx`

- [ ] **Step 1: Fix payment detail page to use useCompletePayment hook**

Replace inline `useMutation` at lines 29-41:
```typescript
import { useCompletePayment } from "@/api/hooks/useCompletePayment"
// ...
const completeMutation = useCompletePayment(id)
```
Remove the `import { useMutation, useQueryClient } from "@tanstack/react-query"` and `import api from "@/services/api"` and `import { queryKeys } from "@/lib/query-keys"` if they're no longer needed.

- [ ] **Step 2: Fix reconciliation page to use hooks**

Replace all inline queries/mutations with:
```typescript
import { useReconciliation } from "@/api/hooks/useReconciliation"
import { useUnmatchedTransactions } from "@/api/hooks/useUnmatchedTransactions"
import { useAutoReconcile } from "@/api/hooks/useAutoReconcile"
import { useMatchTransaction } from "@/api/hooks/useMatchTransaction"
import { useImportBankStatement } from "@/api/hooks/useImportBankStatement"
import { usePayments } from "@/api/hooks/usePayments"
```

Replace inline queries:
- `const { data: reconciliations }` → `useReconciliation()`
- `const { data: unmatched }` → `useUnmatchedTransactions()`
- `const { data: payments }` → `usePayments({ status: "COMPLETED", pageSize: "1000" })`
- `importMutation` → `useImportBankStatement()` 
- `autoReconcileMutation` → `useAutoReconcile()`
- `matchTransactionMutation` → `useMatchTransaction()`

- [ ] **Step 3: Fix refund page to use useRefundPayment hook**

Replace inline useMutation:
```typescript
import { useRefundPayment } from "@/api/hooks/useRefundPayment"
// ...
const mutation = useRefundPayment(id)
```
Remove unused imports.

### Task 4: Add Payment Summary Page

**Files:**
- Create: `frontend/src/app/payments/summary/page.tsx`

- [ ] **Step 1: Create summary page**

```tsx
"use client"

import { PageHeader } from "@/components/shared/PageHeader"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { usePaymentSummary } from "@/api/hooks/usePaymentSummary"
import { formatCurrency } from "@/lib/formatters"
import { DollarSign, TrendingUp, TrendingDown, Activity } from "lucide-react"

export default function PaymentSummaryPage() {
  const { data, isLoading } = usePaymentSummary()

  const stats = [
    { label: "Total Payments", value: data?.totalPayments ?? 0, icon: DollarSign, color: "text-blue-600" },
    { label: "Total Amount", value: data ? formatCurrency(data.totalAmount, "USD") : "-", icon: Activity, color: "text-green-600" },
    { label: "Completed", value: data ? formatCurrency(data.totalCompletedAmount, "USD") : "-", icon: TrendingUp, color: "text-emerald-600" },
    { label: "Refunded", value: data ? formatCurrency(data.totalRefundedAmount, "USD") : "-", icon: TrendingDown, color: "text-red-600" },
    { label: "Net", value: data ? formatCurrency(data.netAmount, "USD") : "-", icon: DollarSign, color: "text-purple-600" },
    { label: "Pending", value: data?.pendingCount ?? 0, icon: Activity, color: "text-amber-600" },
    { label: "Failed", value: data?.failedCount ?? 0, icon: Activity, color: "text-red-600" },
    { label: "Refunded (count)", value: data?.refundedCount ?? 0, icon: TrendingDown, color: "text-orange-600" },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="Payment Summary" backHref="/payments" />
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        {stats.map((s) => (
          <Card key={s.label}>
            <CardHeader className="flex flex-row items-center justify-between pb-2">
              <CardTitle className="text-sm font-medium">{s.label}</CardTitle>
              <s.icon className={`h-4 w-4 ${s.color}`} />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{isLoading ? "..." : s.value}</div>
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  )
}
```

### Task 5: Add Gateway Management Pages

**Files:**
- Create: `frontend/src/app/payments/gateways/page.tsx`
- Create: `frontend/src/app/payments/gateways/new/page.tsx`

- [ ] **Step 1: Create gateway list page**

```tsx
"use client"

import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Card, CardContent } from "@/components/ui/card"
import { usePaymentGateways } from "@/api/hooks/usePaymentGateways"
import type { PaymentGatewayInfo } from "@/api/generated/dto"
import { CreditCard } from "lucide-react"

export default function PaymentGatewaysPage() {
  const { data, isLoading } = usePaymentGateways()

  const columns: Column<PaymentGatewayInfo>[] = [
    { id: "provider", header: "Provider", accessorKey: "provider" },
    { id: "displayName", header: "Name", accessorKey: "displayName" },
    { id: "isAvailable", header: "Available", cell: (row) => <StatusBadge status={row.isAvailable ? "ACTIVE" : "INACTIVE"} /> },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="Payment Gateways" backHref="/payments" createHref="/payments/gateways/new" createLabel="Register Gateway" />
      <Card>
        <CardContent className="pt-6">
          <DataTable
            columns={columns}
            data={data ?? []}
            loading={isLoading}
            emptyTitle="No gateways registered"
            emptyIcon={CreditCard}
            rowKey={(row) => row.provider}
          />
        </CardContent>
      </Card>
    </div>
  )
}
```

- [ ] **Step 2: Create register gateway page**

```tsx
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
import { toast } from "@/components/ui/toast"
import { useCreatePaymentGateway } from "@/api/hooks/useCreatePaymentGateway"
import type { RegisterPaymentGatewayCommand } from "@/api/generated"

const schema = z.object({
  name: z.string().min(1, "Name is required"),
  provider: z.string().min(1, "Provider is required"),
  configuration: z.string().min(1, "Configuration is required"),
  supportedCurrencies: z.string().min(1, "At least one currency required"),
  transactionFee: z.string().min(1, "Fee is required"),
  feeType: z.string().min(1, "Fee type is required"),
  minAmount: z.string().optional(),
  maxAmount: z.string().optional(),
})

type FormData = z.infer<typeof schema>

const providerOptions = [
  { label: "Stripe", value: "Stripe" },
  { label: "PayPal", value: "PayPal" },
  { label: "Local Bank", value: "LocalBank" },
  { label: "Mobile Money", value: "MobileMoney" },
  { label: "Cash", value: "Cash" },
]

const feeTypeOptions = [
  { label: "Fixed", value: "Fixed" },
  { label: "Percentage", value: "Percentage" },
]

export default function RegisterGatewayPage() {
  const router = useRouter()
  const { register, handleSubmit, formState: { errors }, watch, setValue } = useForm<FormData>({
    resolver: zodResolver(schema),
  })
  const mutation = useCreatePaymentGateway()

  return (
    <FormPageLayout title="Register Payment Gateway" backHref="/payments/gateways" onSubmit={handleSubmit((data) => {
      const payload: RegisterPaymentGatewayCommand = {
        name: data.name,
        provider: data.provider,
        configuration: data.configuration,
        supportedCurrencies: data.supportedCurrencies.split(",").map((c) => c.trim()),
        transactionFee: parseFloat(data.transactionFee),
        feeType: data.feeType,
        minAmount: data.minAmount ? parseFloat(data.minAmount) : null,
        maxAmount: data.maxAmount ? parseFloat(data.maxAmount) : null,
      }
      mutation.mutate(payload, {
        onSuccess: () => {
          toast({ title: "Gateway registered" })
          router.push("/payments/gateways")
        },
        onError: () => {
          toast({ title: "Failed to register gateway", variant: "destructive" })
        },
      })
    })}>
      <FormErrorSummary errors={errors} />
      <FormSection title="Gateway Details">
        <FormField label="Name" error={errors.name} registration={register("name")} placeholder="e.g. Main Stripe" required />
        <FormSelectField label="Provider" error={errors.provider} options={providerOptions} value={watch("provider")} onValueChange={(v) => setValue("provider", v)} required placeholder="Select provider" />
        <FormField label="Configuration (JSON)" error={errors.configuration} registration={register("configuration")} placeholder='{"apiKey":"..."}' required />
        <FormField label="Supported Currencies (comma-sep)" error={errors.supportedCurrencies} registration={register("supportedCurrencies")} placeholder="USD,EUR,GBP" required />
        <FormField label="Transaction Fee" error={errors.transactionFee} registration={register("transactionFee")} type="text" placeholder="0.00" required />
        <FormSelectField label="Fee Type" error={errors.feeType} options={feeTypeOptions} value={watch("feeType")} onValueChange={(v) => setValue("feeType", v)} required placeholder="Select type" />
        <FormField label="Min Amount (optional)" error={errors.minAmount} registration={register("minAmount")} type="text" placeholder="0.00" />
        <FormField label="Max Amount (optional)" error={errors.maxAmount} registration={register("maxAmount")} type="text" placeholder="0.00" />
      </FormSection>
      <FormActions backHref="/payments/gateways" loading={mutation.isPending} submitLabel="Register Gateway" />
    </FormPageLayout>
  )
}
```

### Task 6: Update Sidebar Nav

**Files:**
- Modify: `frontend/src/components/shared/ModuleSidebar.tsx`

- [ ] **Step 1: Add summary and gateways to nav**

Find the payments entry and add sub-items or update. Actually, since the sidebar just has a flat list, add the two new pages as separate sidebar entries after Payments:
```typescript
{ href: "/payments", label: "Payments", icon: DollarSign },
{ href: "/payments/summary", label: "Payment Summary", icon: BarChart3 },
{ href: "/payments/gateways", label: "Gateways", icon: CreditCard },
```

Check what icons are available or use existing imports.

### Task 7: Verification

- [ ] **Step 1: Run tsc check**

Run: `cd frontend && bun run lint` — Expected: 0 errors

- [ ] **Step 2: Verify imports resolve**

Check that all imports in the new hooks and pages match the exports in `dto.ts` and `index.ts`.

- [ ] **Step 3: Run build**

Run: `cd frontend && bun run build` — Expected: successful build
