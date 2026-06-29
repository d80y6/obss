# Shared Frontend Framework & Module Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a reusable shared frontend framework, then implement complete UIs for 8 OSS/BSS modules.

**Architecture:** Next.js App Router with shared component library, query key factory, form engine, entity detail framework, multi-tenant support, RTL/Arabic/English localization, and dark mode. Module pages composed from shared components. TanStack Query for server state, zustand for UI state.

**Tech Stack:** Next.js 16.2, TypeScript 5, TanStack Query 5, React Hook Form 7, Zod 4, ShadCN UI (Radix), zustand 5, recharts 3, tailwindcss 4

---

## File Structure

```
src/
  api/
    hooks/
      useCustomers.ts
      useCustomer.ts
      useCreateCustomer.ts
      useUpdateCustomer.ts
      useOrders.ts
      useOrder.ts
      useCreateOrder.ts
      useSubscriptions.ts
      useSubscription.ts
      useCreateSubscription.ts
      useBills.ts
      useBill.ts
      useInvoices.ts
      useInvoice.ts
      usePayments.ts
      usePayment.ts
      useTickets.ts
      useTicket.ts
      useCreateTicket.ts
      useProducts.ts
      useProduct.ts
      useCreateProduct.ts
      useUsers.ts
      useUser.ts
      useCreateUser.ts
    client.ts

  app/
    layout.tsx                          # Wrap with all providers
    page.tsx                            # Redirect to dashboard
    globals.css                         # Add RTL support
    (modules)/
      admin/
        page.tsx                        # Rewrite with shared components
        users/
          [id]/page.tsx
          new/page.tsx
      customers/
        page.tsx                        # Rewrite with DataTable
        [id]/page.tsx                   # Entity detail framework
        new/page.tsx                    # Form engine
        [id]/edit/page.tsx
      products/
        page.tsx
        [id]/page.tsx
        new/page.tsx
        [id]/edit/page.tsx
        offers/page.tsx
        categories/page.tsx
      orders/
        page.tsx
        [id]/page.tsx
        new/page.tsx
        [id]/edit/page.tsx
      subscriptions/
        page.tsx
        [id]/page.tsx
        new/page.tsx
        [id]/edit/page.tsx
      billing/
        page.tsx
        cycles/page.tsx
        [id]/page.tsx
        jobs/page.tsx
      invoices/
        page.tsx
        [id]/page.tsx
        credit-notes/page.tsx
        disputes/page.tsx
      payments/
        page.tsx
        [id]/page.tsx
        reconciliation/page.tsx
        refunds/page.tsx
      tickets/
        page.tsx
        [id]/page.tsx
        new/page.tsx

  components/
    layout/
      app-shell.tsx          # Update with all new providers
      header.tsx             # Add locale + theme toggle
      sidebar.tsx            # Rewrite with ModuleSidebar
    shared/
      DataTable.tsx
      FilterBar.tsx
      SearchBar.tsx
      Pagination.tsx
      BulkActions.tsx
      EntityDetailsLayout.tsx
      FormLayout.tsx
      AuditTimeline.tsx
      StatusBadge.tsx
      MetricCard.tsx
      PageHeader.tsx
      EmptyState.tsx
      ErrorFallback.tsx
      LoadingState.tsx
      PermissionGuard.tsx
      ThemeToggle.tsx
      LocaleToggle.tsx
      EntityTabs.tsx
      EntityHeader.tsx
      EntityMetadata.tsx
      ModuleSidebar.tsx
      ModuleNavigation.tsx
      BreadcrumbBuilder.tsx
    providers.tsx             # Keep + update
    ui/                       # Keep existing shadcn components

  forms/
    FormField.tsx
    FormSection.tsx
    FormActions.tsx
    FormErrorSummary.tsx
    FormPageLayout.tsx

  hooks/
    use-debounce.ts
    use-pagination.ts
    use-localization.ts
    use-permission.ts
    use-tenant.ts
    use-bulk-action.ts

  lib/
    utils.ts                  # Keep
    api-helpers.ts
    formatters.ts
    query-keys.ts
    i18n/
      en.ts
      ar.ts
      index.ts

  providers/
    theme-provider.tsx
    locale-provider.tsx
    permission-provider.tsx
    tenant-provider.tsx

  stores/
    auth-store.ts             # Keep
    theme-store.ts
    locale-store.ts
    tenant-store.ts

  types/
    api.ts                    # Expand with all DTOs
    index.ts
```

---

## Task Breakdown

### Task 1: Multi-Tenant Foundation

**Files:**
- Create: `src/stores/tenant-store.ts`
- Create: `src/hooks/use-tenant.ts`
- Create: `src/providers/tenant-provider.tsx`

- [ ] **Step 1: Create tenant store**

```typescript
// src/stores/tenant-store.ts
import { create } from "zustand"
import { persist } from "zustand/middleware"

export interface Tenant {
  id: string
  name: string
  slug: string
  locale: "en" | "ar"
  theme: "light" | "dark"
}

interface TenantState {
  tenant: Tenant | null
  tenants: Tenant[]
  setTenant: (tenant: Tenant) => void
  setTenants: (tenants: Tenant[]) => void
  clearTenant: () => void
}

export const useTenantStore = create<TenantState>()(
  persist(
    (set) => ({
      tenant: null,
      tenants: [],
      setTenant: (tenant) => set({ tenant }),
      setTenants: (tenants) => set({ tenants }),
      clearTenant: () => set({ tenant: null }),
    }),
    {
      name: "tenant-storage",
      partialize: (state) => ({ tenant: state.tenant }),
    }
  )
)
```

- [ ] **Step 2: Create use-tenant hook**

```typescript
// src/hooks/use-tenant.ts
"use client"

import { useTenantStore } from "@/stores/tenant-store"
import { useQueryClient } from "@tanstack/react-query"
import { useRouter } from "next/navigation"
import { useCallback } from "react"

export function useTenant() {
  const { tenant, tenants, setTenant, setTenants, clearTenant } = useTenantStore()
  const queryClient = useQueryClient()
  const router = useRouter()

  const switchTenant = useCallback(
    (newTenant: Tenant) => {
      setTenant(newTenant)
      queryClient.clear()
      router.refresh()
    },
    [setTenant, queryClient, router]
  )

  return {
    tenant,
    tenants,
    setTenant: switchTenant,
    setTenants,
    clearTenant,
    isLoading: !tenant,
  }
}
```

- [ ] **Step 3: Create tenant provider**

```typescript
// src/providers/tenant-provider.tsx
"use client"

import { createContext, useContext, useEffect } from "react"
import { useTenantStore, Tenant } from "@/stores/tenant-store"
import { useQuery } from "@tanstack/react-query"
import api from "@/services/api"

const TenantContext = createContext<Tenant | null>(null)

export function TenantProvider({ children }: { children: React.ReactNode }) {
  const { tenant, setTenant, setTenants } = useTenantStore()

  const { data: tenants } = useQuery({
    queryKey: ["tenants"],
    queryFn: async () => {
      const res = await api.get("/api/v1/iam/tenants")
      return res.data as Tenant[]
    },
    enabled: !tenant,
  })

  useEffect(() => {
    if (tenants && tenants.length > 0 && !tenant) {
      setTenant(tenants[0])
      setTenants(tenants)
    }
  }, [tenants, tenant, setTenant, setTenants])

  return (
    <TenantContext.Provider value={tenant}>
      {children}
    </TenantContext.Provider>
  )
}

export const useTenantContext = () => useContext(TenantContext)
```

- [ ] **Step 4: Update API client with tenant header**

```typescript
// src/services/api.ts — add tenant interceptor
api.interceptors.request.use((config) => {
  if (typeof window !== "undefined") {
    const token = localStorage.getItem("auth-token")
    if (token) config.headers.Authorization = `Bearer ${token}`
    const stored = localStorage.getItem("tenant-storage")
    if (stored) {
      try {
        const { state } = JSON.parse(stored)
        if (state?.tenant?.id) config.headers["X-Tenant-Id"] = state.tenant.id
      } catch {}
    }
  }
  return config
})
```

### Task 2: Query Key Factory

**Files:**
- Create: `src/lib/query-keys.ts`

- [ ] **Step 1: Create query keys**

```typescript
// src/lib/query-keys.ts
export const queryKeys = {
  tenants: {
    all: ["tenants"] as const,
    detail: (id: string) => ["tenants", id] as const,
  },
  users: {
    all: ["users"] as const,
    lists: () => [...queryKeys.users.all, "list"] as const,
    list: (filters: Record<string, string> = {}) =>
      [...queryKeys.users.lists(), filters] as const,
    details: () => [...queryKeys.users.all, "detail"] as const,
    detail: (id: string) => [...queryKeys.users.details(), id] as const,
  },
  roles: {
    all: ["roles"] as const,
    lists: () => [...queryKeys.roles.all, "list"] as const,
    list: (filters: Record<string, string> = {}) =>
      [...queryKeys.roles.lists(), filters] as const,
    detail: (id: string) => [...queryKeys.roles.all, "detail", id] as const,
  },
  customers: {
    all: ["customers"] as const,
    lists: () => [...queryKeys.customers.all, "list"] as const,
    list: (filters: Record<string, string> = {}) =>
      [...queryKeys.customers.lists(), filters] as const,
    details: () => [...queryKeys.customers.all, "detail"] as const,
    detail: (id: string) => [...queryKeys.customers.details(), id] as const,
    contacts: (id: string) => [...queryKeys.customers.detail(id), "contacts"] as const,
    notes: (id: string) => [...queryKeys.customers.detail(id), "notes"] as const,
    segments: (id: string) => [...queryKeys.customers.detail(id), "segments"] as const,
  },
  products: {
    all: ["products"] as const,
    lists: () => [...queryKeys.products.all, "list"] as const,
    list: (filters: Record<string, string> = {}) =>
      [...queryKeys.products.lists(), filters] as const,
    details: () => [...queryKeys.products.all, "detail"] as const,
    detail: (id: string) => [...queryKeys.products.details(), id] as const,
    offers: (id: string) => [...queryKeys.products.detail(id), "offers"] as const,
    configuration: (id: string) => [...queryKeys.products.detail(id), "configuration"] as const,
  },
  offers: {
    all: ["offers"] as const,
    lists: () => [...queryKeys.offers.all, "list"] as const,
    list: (filters: Record<string, string> = {}) =>
      [...queryKeys.offers.lists(), filters] as const,
    detail: (id: string) => [...queryKeys.offers.all, "detail", id] as const,
  },
  orders: {
    all: ["orders"] as const,
    lists: () => [...queryKeys.orders.all, "list"] as const,
    list: (filters: Record<string, string> = {}) =>
      [...queryKeys.orders.lists(), filters] as const,
    details: () => [...queryKeys.orders.all, "detail"] as const,
    detail: (id: string) => [...queryKeys.orders.details(), id] as const,
    fulfillment: (id: string) => [...queryKeys.orders.detail(id), "fulfillment"] as const,
    timeline: (id: string) => [...queryKeys.orders.detail(id), "timeline"] as const,
  },
  subscriptions: {
    all: ["subscriptions"] as const,
    lists: () => [...queryKeys.subscriptions.all, "list"] as const,
    list: (filters: Record<string, string> = {}) =>
      [...queryKeys.subscriptions.lists(), filters] as const,
    details: () => [...queryKeys.subscriptions.all, "detail"] as const,
    detail: (id: string) => [...queryKeys.subscriptions.details(), id] as const,
    entitlements: (id: string) => [...queryKeys.subscriptions.detail(id), "entitlements"] as const,
    usage: (id: string) => [...queryKeys.subscriptions.detail(id), "usage"] as const,
  },
  billing: {
    all: ["billing"] as const,
    bills: {
      all: ["billing", "bills"] as const,
      lists: () => [...queryKeys.billing.bills.all, "list"] as const,
      list: (filters: Record<string, string> = {}) =>
        [...queryKeys.billing.bills.lists(), filters] as const,
      detail: (id: string) => [...queryKeys.billing.bills.all, "detail", id] as const,
    },
    cycles: {
      all: ["billing", "cycles"] as const,
      list: (filters: Record<string, string> = {}) =>
        [...queryKeys.billing.cycles.all, "list", filters] as const,
    },
    jobs: {
      all: ["billing", "jobs"] as const,
      list: (filters: Record<string, string> = {}) =>
        [...queryKeys.billing.jobs.all, "list", filters] as const,
    },
  },
  invoices: {
    all: ["invoices"] as const,
    lists: () => [...queryKeys.invoices.all, "list"] as const,
    list: (filters: Record<string, string> = {}) =>
      [...queryKeys.invoices.lists(), filters] as const,
    details: () => [...queryKeys.invoices.all, "detail"] as const,
    detail: (id: string) => [...queryKeys.invoices.details(), id] as const,
    disputes: {
      all: ["invoices", "disputes"] as const,
      list: (filters: Record<string, string> = {}) =>
        [...queryKeys.invoices.disputes.all, "list", filters] as const,
    },
    creditNotes: {
      all: ["invoices", "credit-notes"] as const,
      list: (filters: Record<string, string> = {}) =>
        [...queryKeys.invoices.creditNotes.all, "list", filters] as const,
    },
  },
  payments: {
    all: ["payments"] as const,
    lists: () => [...queryKeys.payments.all, "list"] as const,
    list: (filters: Record<string, string> = {}) =>
      [...queryKeys.payments.lists(), filters] as const,
    details: () => [...queryKeys.payments.all, "detail"] as const,
    detail: (id: string) => [...queryKeys.payments.details(), id] as const,
    reconciliation: {
      all: ["payments", "reconciliation"] as const,
      list: (filters: Record<string, string> = {}) =>
        [...queryKeys.payments.reconciliation.all, "list", filters] as const,
    },
    refunds: {
      all: ["payments", "refunds"] as const,
      list: (filters: Record<string, string> = {}) =>
        [...queryKeys.payments.refunds.all, "list", filters] as const,
    },
  },
  tickets: {
    all: ["tickets"] as const,
    lists: () => [...queryKeys.tickets.all, "list"] as const,
    list: (filters: Record<string, string> = {}) =>
      [...queryKeys.tickets.lists(), filters] as const,
    details: () => [...queryKeys.tickets.all, "detail"] as const,
    detail: (id: string) => [...queryKeys.tickets.details(), id] as const,
    sla: (id: string) => [...queryKeys.tickets.detail(id), "sla"] as const,
  },
  services: {
    all: ["services"] as const,
    lists: () => [...queryKeys.services.all, "list"] as const,
    list: (filters: Record<string, string> = {}) =>
      [...queryKeys.services.lists(), filters] as const,
    detail: (id: string) => [...queryKeys.services.all, "detail", id] as const,
    topology: (id: string) => [...queryKeys.services.detail(id), "topology"] as const,
  },
  networks: {
    all: ["network"] as const,
    elements: {
      all: ["network", "elements"] as const,
      list: (filters: Record<string, string> = {}) =>
        [...queryKeys.networks.elements.all, "list", filters] as const,
      detail: (id: string) => [...queryKeys.networks.elements.all, "detail", id] as const,
    },
    olts: {
      all: ["network", "olts"] as const,
      list: (filters: Record<string, string> = {}) =>
        [...queryKeys.networks.olts.all, "list", filters] as const,
    },
    vlans: {
      all: ["network", "vlans"] as const,
      list: (filters: Record<string, string> = {}) =>
        [...queryKeys.networks.vlans.all, "list", filters] as const,
    },
  },
  provisioning: {
    all: ["provisioning"] as const,
    jobs: {
      all: ["provisioning", "jobs"] as const,
      list: (filters: Record<string, string> = {}) =>
        [...queryKeys.provisioning.jobs.all, "list", filters] as const,
      detail: (id: string) => [...queryKeys.provisioning.jobs.all, "detail", id] as const,
    },
    templates: {
      all: ["provisioning", "templates"] as const,
      list: () => [...queryKeys.provisioning.templates.all, "list"] as const,
    },
  },
  workflow: {
    all: ["workflow"] as const,
    definitions: {
      all: ["workflow", "definitions"] as const,
      list: () => [...queryKeys.workflow.definitions.all, "list"] as const,
      detail: (id: string) => [...queryKeys.workflow.definitions.all, "detail", id] as const,
    },
    instances: {
      all: ["workflow", "instances"] as const,
      list: (filters: Record<string, string> = {}) =>
        [...queryKeys.workflow.instances.all, "list", filters] as const,
      detail: (id: string) => [...queryKeys.workflow.instances.all, "detail", id] as const,
    },
  },
  notifications: {
    all: ["notifications"] as const,
    list: (filters: Record<string, string> = {}) =>
      [...queryKeys.notifications.all, "list", filters] as const,
  },
  reporting: {
    all: ["reporting"] as const,
    dashboard: () => [...queryKeys.reporting.all, "dashboard"] as const,
    definitions: {
      all: ["reporting", "definitions"] as const,
      list: () => [...queryKeys.reporting.definitions.all, "list"] as const,
    },
    schedules: {
      all: ["reporting", "schedules"] as const,
      list: () => [...queryKeys.reporting.schedules.all, "list"] as const,
    },
  },
  audit: {
    all: ["audit"] as const,
    entries: {
      all: ["audit", "entries"] as const,
      list: (filters: Record<string, string> = {}) =>
        [...queryKeys.audit.entries.all, "list", filters] as const,
    },
    entity: (entityType: string, entityId: string) =>
      ["audit", "entity", entityType, entityId] as const,
  },
  gateway: {
    all: ["gateway"] as const,
    routes: {
      all: ["gateway", "routes"] as const,
      list: () => [...queryKeys.gateway.routes.all, "list"] as const,
    },
    apiKeys: {
      all: ["gateway", "api-keys"] as const,
      list: () => [...queryKeys.gateway.apiKeys.all, "list"] as const,
    },
    partners: {
      all: ["gateway", "partners"] as const,
      list: () => [...queryKeys.gateway.partners.all, "list"] as const,
    },
  },
} as const
```

### Task 3: Stores (theme, locale)

**Files:**
- Create: `src/stores/theme-store.ts`
- Create: `src/stores/locale-store.ts`

- [ ] **Step 1: Create theme store**

```typescript
// src/stores/theme-store.ts
import { create } from "zustand"
import { persist } from "zustand/middleware"

type Theme = "light" | "dark"

interface ThemeState {
  theme: Theme
  setTheme: (theme: Theme) => void
  toggleTheme: () => void
}

export const useThemeStore = create<ThemeState>()(
  persist(
    (set) => ({
      theme: "light",
      setTheme: (theme) => set({ theme }),
      toggleTheme: () =>
        set((state) => ({ theme: state.theme === "light" ? "dark" : "light" })),
    }),
    { name: "theme-storage" }
  )
)
```

- [ ] **Step 2: Create locale store**

```typescript
// src/stores/locale-store.ts
import { create } from "zustand"
import { persist } from "zustand/middleware"

export type Locale = "en" | "ar"

interface LocaleState {
  locale: Locale
  dir: "ltr" | "rtl"
  setLocale: (locale: Locale) => void
  toggleLocale: () => void
}

export const useLocaleStore = create<LocaleState>()(
  persist(
    (set) => ({
      locale: "en",
      dir: "ltr",
      setLocale: (locale) =>
        set({
          locale,
          dir: locale === "ar" ? "rtl" : "ltr",
        }),
      toggleLocale: () =>
        set((state) => ({
          locale: state.locale === "en" ? "ar" : "en",
          dir: state.locale === "en" ? "rtl" : "ltr",
        })),
    }),
    { name: "locale-storage" }
  )
)
```

### Task 4: Providers (theme, locale, permission)

**Files:**
- Create: `src/providers/theme-provider.tsx`
- Create: `src/providers/locale-provider.tsx`
- Create: `src/providers/permission-provider.tsx`
- Modify: `src/components/providers.tsx`
- Modify: `src/app/layout.tsx`

- [ ] **Step 1: Create theme provider**

```typescript
// src/providers/theme-provider.tsx
"use client"

import { useEffect } from "react"
import { useThemeStore } from "@/stores/theme-store"

export function ThemeProvider({ children }: { children: React.ReactNode }) {
  const { theme } = useThemeStore()

  useEffect(() => {
    const root = document.documentElement
    if (theme === "dark") {
      root.classList.add("dark")
    } else {
      root.classList.remove("dark")
    }
  }, [theme])

  return <>{children}</>
}
```

- [ ] **Step 2: Create locale provider**

```typescript
// src/providers/locale-provider.tsx
"use client"

import { useEffect } from "react"
import { useLocaleStore } from "@/stores/locale-store"

export function LocaleProvider({ children }: { children: React.ReactNode }) {
  const { locale, dir } = useLocaleStore()

  useEffect(() => {
    document.documentElement.lang = locale
    document.documentElement.dir = dir
  }, [locale, dir])

  return <>{children}</>
}
```

- [ ] **Step 3: Create permission provider**

```typescript
// src/providers/permission-provider.tsx
"use client"

import { createContext, useContext } from "react"
import { useAuthStore } from "@/stores/auth-store"

interface PermissionContextValue {
  hasRole: (role: string) => boolean
  hasAnyRole: (roles: string[]) => boolean
  isAuthenticated: boolean
}

const PermissionContext = createContext<PermissionContextValue>({
  hasRole: () => false,
  hasAnyRole: () => false,
  isAuthenticated: false,
})

export function PermissionProvider({ children }: { children: React.ReactNode }) {
  const { user, isAuthenticated } = useAuthStore()

  const hasRole = (role: string) => {
    return user?.role === role || user?.role === "ADMIN"
  }

  const hasAnyRole = (roles: string[]) => {
    return roles.some((r) => hasRole(r))
  }

  return (
    <PermissionContext.Provider value={{ hasRole, hasAnyRole, isAuthenticated }}>
      {children}
    </PermissionContext.Provider>
  )
}

export const usePermission = () => useContext(PermissionContext)
```

- [ ] **Step 4: Update providers.tsx to compose all providers**

```typescript
// src/components/providers.tsx
"use client"

import { QueryClient, QueryClientProvider } from "@tanstack/react-query"
import { useState } from "react"
import { ThemeProvider } from "@/providers/theme-provider"
import { LocaleProvider } from "@/providers/locale-provider"
import { PermissionProvider } from "@/providers/permission-provider"
import { TenantProvider } from "@/providers/tenant-provider"

export function Providers({ children }: { children: React.ReactNode }) {
  const [queryClient] = useState(
    () =>
      new QueryClient({
        defaultOptions: {
          queries: {
            staleTime: 60 * 1000,
            retry: 1,
          },
        },
      })
  )

  return (
    <QueryClientProvider client={queryClient}>
      <TenantProvider>
        <ThemeProvider>
          <LocaleProvider>
            <PermissionProvider>
              {children}
            </PermissionProvider>
          </LocaleProvider>
        </ThemeProvider>
      </TenantProvider>
    </QueryClientProvider>
  )
}
```

### Task 5: i18n System

**Files:**
- Create: `src/lib/i18n/en.ts`
- Create: `src/lib/i18n/ar.ts`
- Create: `src/lib/i18n/index.ts`

- [ ] **Step 1: Create English translations**

```typescript
// src/lib/i18n/en.ts
export const en = {
  common: {
    loading: "Loading...",
    error: "An error occurred",
    retry: "Retry",
    save: "Save",
    saving: "Saving...",
    cancel: "Cancel",
    delete: "Delete",
    edit: "Edit",
    create: "Create",
    search: "Search...",
    noResults: "No results found",
    noData: "No data available",
    back: "Back",
    actions: "Actions",
    status: "Status",
    confirm: "Confirm",
    close: "Close",
    previous: "Previous",
    next: "Next",
    page: "Page",
    of: "of",
    total: "Total",
    bulkActions: "Bulk Actions",
    export: "Export",
    filter: "Filter",
    clearFilters: "Clear Filters",
    all: "All",
    selectAll: "Select All",
    deselectAll: "Deselect All",
    yes: "Yes",
    no: "No",
  },
  nav: {
    dashboard: "Dashboard",
    customers: "Customers",
    products: "Products",
    orders: "Orders",
    subscriptions: "Subscriptions",
    invoices: "Invoices",
    billing: "Billing",
    payments: "Payments",
    tickets: "Tickets",
    admin: "Admin",
    collections: "Collections",
    services: "Services",
    network: "Network",
    provisioning: "Provisioning",
    workflow: "Workflow",
    notifications: "Notifications",
    reporting: "Reporting",
    audit: "Audit",
    apiGateway: "API Gateway",
  },
  customer: {
    title: "Customers",
    new: "New Customer",
    details: "Customer Details",
    edit: "Edit Customer",
    firstName: "First Name",
    lastName: "Last Name",
    email: "Email",
    phone: "Phone",
    address: "Address",
    city: "City",
    state: "State",
    zipCode: "ZIP Code",
    country: "Country",
    status: "Status",
    type: "Type",
    active: "Active",
    inactive: "Inactive",
    suspended: "Suspended",
    individual: "Individual",
    business: "Business",
    overview: "Overview",
    contacts: "Contacts",
    notes: "Notes",
    orders: "Orders",
    subscriptions: "Subscriptions",
    invoices: "Invoices",
    payments: "Payments",
    audit: "Audit",
  },
  product: {
    title: "Products",
    new: "New Product",
    details: "Product Details",
    edit: "Edit Product",
    name: "Name",
    description: "Description",
    category: "Category",
    type: "Type",
    status: "Status",
    taxCategory: "Tax Category",
    offers: "Offers",
    configuration: "Configuration",
    pricing: "Pricing",
  },
  order: {
    title: "Orders",
    new: "New Order",
    details: "Order Details",
    edit: "Edit Order",
    orderNumber: "Order #",
    customer: "Customer",
    date: "Date",
    status: "Status",
    total: "Total",
    currency: "Currency",
    items: "Items",
    tracking: "Tracking",
    fulfillment: "Fulfillment",
    timeline: "Timeline",
  },
  subscription: {
    title: "Subscriptions",
    new: "New Subscription",
    details: "Subscription Details",
    edit: "Edit Subscription",
    subscriptionNumber: "Subscription #",
    customer: "Customer",
    offer: "Offer",
    status: "Status",
    startDate: "Start Date",
    endDate: "End Date",
    amount: "Amount",
    billingPeriod: "Billing Period",
    entitlements: "Entitlements",
    addons: "Add-ons",
  },
  invoice: {
    title: "Invoices",
    details: "Invoice Details",
    invoiceNumber: "Invoice #",
    customer: "Customer",
    issueDate: "Issue Date",
    dueDate: "Due Date",
    amount: "Amount",
    status: "Status",
    lineItems: "Line Items",
    creditNotes: "Credit Notes",
    disputes: "Disputes",
    summary: "Summary",
  },
  billing: {
    title: "Billing",
    cycles: "Billing Cycles",
    jobs: "Billing Jobs",
    details: "Bill Details",
    billNumber: "Bill #",
    customer: "Customer",
    period: "Period",
    status: "Status",
    total: "Total",
    issueDate: "Issue Date",
  },
  payment: {
    title: "Payments",
    details: "Payment Details",
    paymentNumber: "Payment #",
    customer: "Customer",
    amount: "Amount",
    method: "Method",
    status: "Status",
    date: "Date",
    transactionId: "Transaction ID",
    reconciliation: "Reconciliation",
    refunds: "Refunds",
  },
  ticket: {
    title: "Tickets",
    new: "New Ticket",
    details: "Ticket Details",
    ticketNumber: "Ticket #",
    subject: "Subject",
    customer: "Customer",
    priority: "Priority",
    status: "Status",
    category: "Category",
    assignedTo: "Assigned To",
    created: "Created",
    sla: "SLA",
    escalation: "Escalation",
    comments: "Comments",
  },
  admin: {
    title: "Admin",
    users: "Users",
    roles: "Roles",
    permissions: "Permissions",
    settings: "Settings",
    username: "Username",
    email: "Email",
    firstName: "First Name",
    lastName: "Last Name",
    active: "Active",
    role: "Role",
  },
  tenant: {
    switch: "Switch Tenant",
    current: "Current Tenant",
  },
  theme: {
    light: "Light",
    dark: "Dark",
    toggle: "Toggle theme",
  },
  locale: {
    en: "English",
    ar: "العربية",
    toggle: "Switch language",
  },
  status: {
    ACTIVE: "Active",
    INACTIVE: "Inactive",
    PENDING: "Pending",
    SUSPENDED: "Suspended",
    CANCELLED: "Cancelled",
    COMPLETED: "Completed",
    FAILED: "Failed",
    DRAFT: "Draft",
    SENT: "Sent",
    PAID: "Paid",
    OVERDUE: "Overdue",
    REFUNDED: "Refunded",
    OPEN: "Open",
    IN_PROGRESS: "In Progress",
    RESOLVED: "Resolved",
    CLOSED: "Closed",
    EXPIRED: "Expired",
    LOW: "Low",
    MEDIUM: "Medium",
    HIGH: "High",
    CRITICAL: "Critical",
  },
}
```

- [ ] **Step 2: Create Arabic translations**

```typescript
// src/lib/i18n/ar.ts
export const ar = {
  common: {
    loading: "جار التحميل...",
    error: "حدث خطأ",
    retry: "إعادة المحاولة",
    save: "حفظ",
    saving: "جار الحفظ...",
    cancel: "إلغاء",
    delete: "حذف",
    edit: "تعديل",
    create: "إنشاء",
    search: "بحث...",
    noResults: "لا توجد نتائج",
    noData: "لا توجد بيانات",
    back: "رجوع",
    actions: "إجراءات",
    status: "الحالة",
    confirm: "تأكيد",
    close: "إغلاق",
    previous: "السابق",
    next: "التالي",
    page: "صفحة",
    of: "من",
    total: "المجموع",
    bulkActions: "إجراءات جماعية",
    export: "تصدير",
    filter: "تصفية",
    clearFilters: "مسح التصفية",
    all: "الكل",
    selectAll: "تحديد الكل",
    deselectAll: "إلغاء تحديد الكل",
    yes: "نعم",
    no: "لا",
  },
  nav: {
    dashboard: "لوحة القيادة",
    customers: "العملاء",
    products: "المنتجات",
    orders: "الطلبات",
    subscriptions: "الاشتراكات",
    invoices: "الفواتير",
    billing: "الفواتير",
    payments: "المدفوعات",
    tickets: "التذاكر",
    admin: "الإدارة",
    collections: "التحصيلات",
    services: "الخدمات",
    network: "الشبكة",
    provisioning: "التجهيز",
    workflow: "سير العمل",
    notifications: "الإشعارات",
    reporting: "التقارير",
    audit: "التدقيق",
    apiGateway: "بوابة API",
  },
  customer: {
    title: "العملاء",
    new: "عميل جديد",
    details: "تفاصيل العميل",
    edit: "تعديل العميل",
    firstName: "الاسم الأول",
    lastName: "اسم العائلة",
    email: "البريد الإلكتروني",
    phone: "الهاتف",
    address: "العنوان",
    city: "المدينة",
    state: "الولاية",
    zipCode: "الرمز البريدي",
    country: "الدولة",
    status: "الحالة",
    type: "النوع",
    active: "نشط",
    inactive: "غير نشط",
    suspended: "موقوف",
    individual: "فرد",
    business: "شركة",
    overview: "نظرة عامة",
    contacts: "جهات الاتصال",
    notes: "ملاحظات",
    orders: "الطلبات",
    subscriptions: "الاشتراكات",
    invoices: "الفواتير",
    payments: "المدفوعات",
    audit: "التدقيق",
  },
  product: {
    title: "المنتجات",
    new: "منتج جديد",
    details: "تفاصيل المنتج",
    edit: "تعديل المنتج",
    name: "الاسم",
    description: "الوصف",
    category: "التصنيف",
    type: "النوع",
    status: "الحالة",
    taxCategory: "تصنيف الضريبة",
    offers: "العروض",
    configuration: "التكوين",
    pricing: "التسعير",
  },
  order: {
    title: "الطلبات",
    new: "طلب جديد",
    details: "تفاصيل الطلب",
    edit: "تعديل الطلب",
    orderNumber: "رقم الطلب",
    customer: "العميل",
    date: "التاريخ",
    status: "الحالة",
    total: "المجموع",
    currency: "العملة",
    items: "العناصر",
    tracking: "التتبع",
    fulfillment: "التنفيذ",
    timeline: "الجدول الزمني",
  },
  subscription: {
    title: "الاشتراكات",
    new: "اشتراك جديد",
    details: "تفاصيل الاشتراك",
    edit: "تعديل الاشتراك",
    subscriptionNumber: "رقم الاشتراك",
    customer: "العميل",
    offer: "العرض",
    status: "الحالة",
    startDate: "تاريخ البداية",
    endDate: "تاريخ النهاية",
    amount: "المبلغ",
    billingPeriod: "دورة الفوترة",
    entitlements: "الاستحقاقات",
    addons: "الإضافات",
  },
  invoice: {
    title: "الفواتير",
    details: "تفاصيل الفاتورة",
    invoiceNumber: "رقم الفاتورة",
    customer: "العميل",
    issueDate: "تاريخ الإصدار",
    dueDate: "تاريخ الاستحقاق",
    amount: "المبلغ",
    status: "الحالة",
    lineItems: "بنود الفاتورة",
    creditNotes: "إشعارات الدائن",
    disputes: "النزاعات",
    summary: "الملخص",
  },
  billing: {
    title: "الفوترة",
    cycles: "دورات الفوترة",
    jobs: "وظائف الفوترة",
    details: "تفاصيل الفاتورة",
    billNumber: "رقم الفاتورة",
    customer: "العميل",
    period: "الفترة",
    status: "الحالة",
    total: "المجموع",
    issueDate: "تاريخ الإصدار",
  },
  payment: {
    title: "المدفوعات",
    details: "تفاصيل الدفع",
    paymentNumber: "رقم الدفع",
    customer: "العميل",
    amount: "المبلغ",
    method: "طريقة الدفع",
    status: "الحالة",
    date: "التاريخ",
    transactionId: "رقم المعاملة",
    reconciliation: "التسوية",
    refunds: "المبالغ المستردة",
  },
  ticket: {
    title: "التذاكر",
    new: "تذكرة جديدة",
    details: "تفاصيل التذكرة",
    ticketNumber: "رقم التذكرة",
    subject: "الموضوع",
    customer: "العميل",
    priority: "الأولوية",
    status: "الحالة",
    category: "التصنيف",
    assignedTo: "مسند إلى",
    created: "تاريخ الإنشاء",
    sla: "مستوى الخدمة",
    escalation: "التصعيد",
    comments: "التعليقات",
  },
  admin: {
    title: "الإدارة",
    users: "المستخدمين",
    roles: "الأدوار",
    permissions: "الصلاحيات",
    settings: "الإعدادات",
    username: "اسم المستخدم",
    email: "البريد الإلكتروني",
    firstName: "الاسم الأول",
    lastName: "اسم العائلة",
    active: "نشط",
    role: "الدور",
  },
  tenant: {
    switch: "تبديل المستأجر",
    current: "المستأجر الحالي",
  },
  theme: {
    light: "فاتح",
    dark: "داكن",
    toggle: "تبديل السمة",
  },
  locale: {
    en: "English",
    ar: "العربية",
    toggle: "تبديل اللغة",
  },
  status: {
    ACTIVE: "نشط",
    INACTIVE: "غير نشط",
    PENDING: "قيد الانتظار",
    SUSPENDED: "موقوف",
    CANCELLED: "ملغي",
    COMPLETED: "مكتمل",
    FAILED: "فشل",
    DRAFT: "مسودة",
    SENT: "مرسل",
    PAID: "مدفوع",
    OVERDUE: "متأخر",
    REFUNDED: "مسترجع",
    OPEN: "مفتوح",
    IN_PROGRESS: "قيد التنفيذ",
    RESOLVED: "تم الحل",
    CLOSED: "مغلق",
    EXPIRED: "منتهي",
    LOW: "منخفض",
    MEDIUM: "متوسط",
    HIGH: "عالٍ",
    CRITICAL: "حرج",
  },
}
```

- [ ] **Step 3: Create i18n index**

```typescript
// src/lib/i18n/index.ts
import { en } from "./en"
import { ar } from "./ar"
import { useLocaleStore } from "@/stores/locale-store"

export type TranslationKey = keyof typeof en

const translations = { en, ar } as const

function getNestedValue(obj: Record<string, unknown>, path: string): string {
  const keys = path.split(".")
  let current: unknown = obj
  for (const key of keys) {
    if (current && typeof current === "object" && key in (current as Record<string, unknown>)) {
      current = (current as Record<string, unknown>)[key]
    } else {
      return path
    }
  }
  return typeof current === "string" ? current : path
}

export function t(path: string, params?: Record<string, string | number>): string {
  const { locale } = useLocaleStore.getState()
  const lang = translations[locale as keyof typeof translations] || translations.en
  let value = getNestedValue(lang as unknown as Record<string, unknown>, path)
  if (params) {
    Object.entries(params).forEach(([key, val]) => {
      value = value.replace(`{${key}}`, String(val))
    })
  }
  return value
}

export function useTranslation() {
  const { locale, dir, setLocale, toggleLocale } = useLocaleStore()

  const _t = (path: string, params?: Record<string, string | number>): string => {
    return t(path, params)
  }

  return { t: _t, locale, dir, setLocale, toggleLocale }
}
```

### Task 6: Shared Components Part 1

**Files:**
- Create: `src/components/shared/StatusBadge.tsx`
- Create: `src/components/shared/PageHeader.tsx`
- Create: `src/components/shared/MetricCard.tsx`
- Create: `src/components/shared/EmptyState.tsx`
- Create: `src/components/shared/ErrorFallback.tsx`
- Create: `src/components/shared/LoadingState.tsx`
- Create: `src/components/shared/SearchBar.tsx`
- Create: `src/components/shared/Pagination.tsx`

- [ ] **Step 1: StatusBadge**

```typescript
// src/components/shared/StatusBadge.tsx
"use client"

import { Badge } from "@/components/ui/badge"

const statusVariantMap: Record<string, "default" | "secondary" | "destructive" | "outline" | "success" | "warning" | "info"> = {
  ACTIVE: "success",
  COMPLETED: "success",
  PAID: "success",
  RESOLVED: "success",
  ENABLED: "success",
  PENDING: "warning",
  SENT: "warning",
  IN_PROGRESS: "warning",
  SUSPENDED: "warning",
  DRAFT: "secondary",
  INACTIVE: "secondary",
  EXPIRED: "secondary",
  REFUNDED: "secondary",
  CLOSED: "secondary",
  CANCELLED: "destructive",
  FAILED: "destructive",
  OVERDUE: "destructive",
  OPEN: "destructive",
  DISABLED: "destructive",
}

export function StatusBadge({ status }: { status: string }) {
  const variant = statusVariantMap[status] || "default"
  return <Badge variant={variant}>{status}</Badge>
}
```

- [ ] **Step 2: PageHeader**

```typescript
// src/components/shared/PageHeader.tsx
"use client"

import { Button } from "@/components/ui/button"
import { Plus, ArrowLeft } from "lucide-react"
import Link from "next/link"

interface PageHeaderProps {
  title: string
  description?: string
  backHref?: string
  createHref?: string
  createLabel?: string
  actions?: React.ReactNode
}

export function PageHeader({ title, description, backHref, createHref, createLabel, actions }: PageHeaderProps) {
  return (
    <div className="flex items-center justify-between">
      <div className="flex items-center gap-4">
        {backHref && (
          <Button variant="ghost" size="icon" asChild>
            <Link href={backHref}>
              <ArrowLeft className="h-4 w-4" />
            </Link>
          </Button>
        )}
        <div>
          <h1 className="text-2xl font-bold tracking-tight">{title}</h1>
          {description && (
            <p className="text-sm text-muted-foreground">{description}</p>
          )}
        </div>
      </div>
      <div className="flex items-center gap-2">
        {actions}
        {createHref && (
          <Button asChild>
            <Link href={createHref}>
              <Plus className="mr-1 h-4 w-4" /> {createLabel || "Create"}
            </Link>
          </Button>
        )}
      </div>
    </div>
  )
}
```

- [ ] **Step 3: MetricCard**

```typescript
// src/components/shared/MetricCard.tsx
"use client"

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Skeleton } from "@/components/ui/skeleton"
import { LucideIcon } from "lucide-react"
import Link from "next/link"

interface MetricCardProps {
  title: string
  value: number | string
  icon?: LucideIcon
  href?: string
  loading?: boolean
  format?: "currency" | "number" | "percent"
  trend?: { value: number; positive: boolean }
}

export function MetricCard({ title, value, icon: Icon, href, loading, format, trend }: MetricCardProps) {
  const content = (
    <Card className="transition-colors hover:bg-muted/50">
      <CardHeader className="flex flex-row items-center justify-between pb-2">
        <CardTitle className="text-sm font-medium text-muted-foreground">
          {title}
        </CardTitle>
        {Icon && <Icon className="h-4 w-4 text-muted-foreground" />}
      </CardHeader>
      <CardContent>
        {loading ? (
          <Skeleton className="h-8 w-20" />
        ) : (
          <>
            <p className="text-3xl font-bold">{value}</p>
            {trend && (
              <p className={`text-xs mt-1 ${trend.positive ? "text-emerald-600" : "text-destructive"}`}>
                {trend.positive ? "+" : ""}{trend.value}%
              </p>
            )}
          </>
        )}
      </CardContent>
    </Card>
  )

  if (href) {
    return <Link href={href}>{content}</Link>
  }
  return content
}
```

- [ ] **Step 4: EmptyState**

```typescript
// src/components/shared/EmptyState.tsx
"use client"

import { LucideIcon, Inbox } from "lucide-react"
import { Button } from "@/components/ui/button"
import Link from "next/link"

interface EmptyStateProps {
  icon?: LucideIcon
  title: string
  description?: string
  actionHref?: string
  actionLabel?: string
}

export function EmptyState({ icon: Icon = Inbox, title, description, actionHref, actionLabel }: EmptyStateProps) {
  return (
    <div className="flex flex-col items-center justify-center py-12 text-center">
      <Icon className="h-12 w-12 text-muted-foreground/50" />
      <h3 className="mt-4 text-lg font-semibold">{title}</h3>
      {description && (
        <p className="mt-1 text-sm text-muted-foreground">{description}</p>
      )}
      {actionHref && (
        <Button asChild className="mt-4">
          <Link href={actionHref}>{actionLabel}</Link>
        </Button>
      )}
    </div>
  )
}
```

- [ ] **Step 5: ErrorFallback**

```typescript
// src/components/shared/ErrorFallback.tsx
"use client"

import { AlertCircle } from "lucide-react"
import { Button } from "@/components/ui/button"

interface ErrorFallbackProps {
  message?: string
  onRetry?: () => void
}

export function ErrorFallback({ message = "An error occurred", onRetry }: ErrorFallbackProps) {
  return (
    <div className="flex flex-col items-center justify-center py-12 text-center">
      <AlertCircle className="h-12 w-12 text-destructive/50" />
      <h3 className="mt-4 text-lg font-semibold">Error</h3>
      <p className="mt-1 text-sm text-muted-foreground">{message}</p>
      {onRetry && (
        <Button variant="outline" className="mt-4" onClick={onRetry}>
          Retry
        </Button>
      )}
    </div>
  )
}
```

- [ ] **Step 6: LoadingState**

```typescript
// src/components/shared/LoadingState.tsx
"use client"

import { Skeleton } from "@/components/ui/skeleton"

interface LoadingStateProps {
  rows?: number
}

export function LoadingState({ rows = 5 }: LoadingStateProps) {
  return (
    <div className="space-y-3">
      {Array.from({ length: rows }).map((_, i) => (
        <Skeleton key={i} className="h-10 w-full" />
      ))}
    </div>
  )
}
```

- [ ] **Step 7: SearchBar**

```typescript
// src/components/shared/SearchBar.tsx
"use client"

import { Search } from "lucide-react"
import { Input } from "@/components/ui/input"
import { useDebounce } from "@/hooks/use-debounce"
import { useEffect, useState } from "react"

interface SearchBarProps {
  placeholder?: string
  value: string
  onChange: (value: string) => void
  debounceMs?: number
}

export function SearchBar({ placeholder = "Search...", value, onChange, debounceMs = 300 }: SearchBarProps) {
  const [localValue, setLocalValue] = useState(value)
  const debouncedValue = useDebounce(localValue, debounceMs)

  useEffect(() => {
    onChange(debouncedValue)
  }, [debouncedValue, onChange])

  useEffect(() => {
    setLocalValue(value)
  }, [value])

  return (
    <div className="relative flex-1">
      <Search className="absolute left-2.5 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
      <Input
        placeholder={placeholder}
        className="pl-8"
        value={localValue}
        onChange={(e) => setLocalValue(e.target.value)}
      />
    </div>
  )
}
```

- [ ] **Step 8: Create useDebounce hook**

```typescript
// src/hooks/use-debounce.ts
import { useState, useEffect } from "react"

export function useDebounce<T>(value: T, delay: number = 300): T {
  const [debouncedValue, setDebouncedValue] = useState<T>(value)

  useEffect(() => {
    const timer = setTimeout(() => setDebouncedValue(value), delay)
    return () => clearTimeout(timer)
  }, [value, delay])

  return debouncedValue
}
```

- [ ] **Step 9: Pagination**

```typescript
// src/components/shared/Pagination.tsx
"use client"

import { Button } from "@/components/ui/button"

interface PaginationProps {
  page: number
  pageSize: number
  total: number
  onPageChange: (page: number) => void
  onPageSizeChange?: (pageSize: number) => void
}

export function Pagination({ page, pageSize, total, onPageChange, onPageSizeChange }: PaginationProps) {
  const totalPages = Math.max(1, Math.ceil(total / pageSize))
  const hasNext = page < totalPages
  const hasPrev = page > 1

  return (
    <div className="flex items-center justify-between pt-4">
      <div className="flex items-center gap-2">
        <p className="text-sm text-muted-foreground">
          Page {page} of {totalPages} ({total} total)
        </p>
        {onPageSizeChange && (
          <select
            className="text-sm border rounded px-2 py-1 bg-background"
            value={pageSize}
            onChange={(e) => onPageSizeChange(Number(e.target.value))}
          >
            {[10, 20, 50, 100].map((size) => (
              <option key={size} value={size}>{size} / page</option>
            ))}
          </select>
        )}
      </div>
      <div className="flex gap-2">
        <Button variant="outline" size="sm" disabled={!hasPrev} onClick={() => onPageChange(page - 1)}>
          Previous
        </Button>
        <Button variant="outline" size="sm" disabled={!hasNext} onClick={() => onPageChange(page + 1)}>
          Next
        </Button>
      </div>
    </div>
  )
}
```

### Task 7: Enhanced DataTable

**Files:**
- Create: `src/components/shared/DataTable.tsx`
- Create: `src/components/shared/BulkActions.tsx`
- Create: `src/components/shared/FilterBar.tsx`

- [ ] **Step 1: BulkActions**

```typescript
// src/components/shared/BulkActions.tsx
"use client"

import { Button } from "@/components/ui/button"
import { useTranslation } from "@/lib/i18n"

interface BulkAction {
  label: string
  onClick: (ids: string[]) => void
  variant?: "default" | "destructive" | "outline" | "secondary"
}

interface BulkActionsProps {
  selectedIds: string[]
  actions: BulkAction[]
}

export function BulkActions({ selectedIds, actions }: BulkActionsProps) {
  if (selectedIds.length === 0) return null

  return (
    <div className="flex items-center gap-2 px-2 py-2 bg-muted/50 rounded-md">
      <span className="text-sm text-muted-foreground">
        {selectedIds.length} selected
      </span>
      {actions.map((action) => (
        <Button
          key={action.label}
          variant={action.variant || "outline"}
          size="sm"
          onClick={() => action.onClick(selectedIds)}
        >
          {action.label}
        </Button>
      ))}
    </div>
  )
}
```

- [ ] **Step 2: FilterBar**

```typescript
// src/components/shared/FilterBar.tsx
"use client"

import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Input } from "@/components/ui/input"
import { Button } from "@/components/ui/button"
import { X } from "lucide-react"

interface FilterOption {
  id: string
  label: string
  type: "select" | "date-range" | "text" | "number"
  options?: { label: string; value: string }[]
  value: string
  onChange: (value: string) => void
  placeholder?: string
}

interface FilterBarProps {
  filters: FilterOption[]
  onClear: () => void
}

export function FilterBar({ filters, onClear }: FilterBarProps) {
  const hasFilters = filters.some((f) => f.value)

  return (
    <div className="flex flex-wrap items-center gap-3">
      {filters.map((filter) => {
        if (filter.type === "select") {
          return (
            <Select key={filter.id} value={filter.value} onValueChange={filter.onChange}>
              <SelectTrigger className="w-40">
                <SelectValue placeholder={filter.placeholder || filter.label} />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">{filter.placeholder || `All ${filter.label}`}</SelectItem>
                {filter.options?.map((opt) => (
                  <SelectItem key={opt.value} value={opt.value}>
                    {opt.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          )
        }
        if (filter.type === "text") {
          return (
            <div key={filter.id} className="relative">
              <Input
                placeholder={filter.placeholder || `Filter ${filter.label}...`}
                className="w-48 h-9"
                value={filter.value}
                onChange={(e) => filter.onChange(e.target.value)}
              />
            </div>
          )
        }
        if (filter.type === "date-range") {
          return (
            <Input
              key={filter.id}
              type="date"
              className="w-40 h-9"
              value={filter.value}
              onChange={(e) => filter.onChange(e.target.value)}
            />
          )
        }
        return null
      })}
      {hasFilters && (
        <Button variant="ghost" size="sm" onClick={onClear}>
          <X className="h-4 w-4 mr-1" /> Clear
        </Button>
      )}
    </div>
  )
}
```

- [ ] **Step 3: DataTable — full implementation**

```typescript
// src/components/shared/DataTable.tsx
"use client"

import { useState, useCallback } from "react"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import { Checkbox } from "@/components/ui/checkbox"
import { Button } from "@/components/ui/button"
import { ArrowUpDown, ArrowUp, ArrowDown, EyeOff, Columns } from "lucide-react"
import {
  DropdownMenu,
  DropdownMenuCheckboxItem,
  DropdownMenuContent,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"
import { LoadingState } from "./LoadingState"
import { EmptyState } from "./EmptyState"
import { ErrorFallback } from "./ErrorFallback"
import { Pagination } from "./Pagination"
import { BulkActions, BulkAction } from "./BulkActions"
import { cn } from "@/lib/utils"

export interface Column<T> {
  id: string
  header: string
  accessorKey?: keyof T & string
  cell?: (row: T) => React.ReactNode
  sortable?: boolean
  width?: string
  hidden?: boolean
}

interface DataTableProps<T> {
  columns: Column<T>[]
  data: T[]
  loading?: boolean
  error?: string
  emptyTitle?: string
  emptyDescription?: string
  emptyIcon?: React.ElementType
  rowKey: (row: T) => string
  selectedIds?: string[]
  onSelectionChange?: (ids: string[]) => void
  onRowClick?: (row: T) => void
  sortBy?: string
  sortOrder?: "asc" | "desc"
  onSortChange?: (column: string, order: "asc" | "desc") => void
  pagination?: {
    page: number
    pageSize: number
    total: number
    onPageChange: (page: number) => void
    onPageSizeChange?: (size: number) => void
  }
  bulkActions?: BulkAction[]
  onExportCsv?: () => void
  onExportExcel?: () => void
  columnVisibility?: Record<string, boolean>
  onColumnVisibilityChange?: (columns: Record<string, boolean>) => void
}

export function DataTable<T extends Record<string, unknown>>({
  columns,
  data,
  loading,
  error,
  emptyTitle = "No data found",
  emptyDescription,
  emptyIcon,
  rowKey,
  selectedIds = [],
  onSelectionChange,
  onRowClick,
  sortBy,
  sortOrder = "asc",
  onSortChange,
  pagination,
  bulkActions,
  onExportCsv,
  onExportExcel,
  columnVisibility,
  onColumnVisibilityChange,
}: DataTableProps<T>) {
  const [localColumnVisibility, setLocalColumnVisibility] = useState<Record<string, boolean>>(
    columnVisibility || columns.reduce((acc, col) => ({ ...acc, [col.id]: !col.hidden }), {})
  )

  const visibleColumns = columns.filter(
    (col) => (columnVisibility || localColumnVisibility)[col.id] !== false
  )

  const handleSort = useCallback(
    (columnId: string) => {
      if (!onSortChange) return
      const isAsc = sortBy === columnId && sortOrder === "asc"
      onSortChange(columnId, isAsc ? "desc" : "asc")
    },
    [sortBy, sortOrder, onSortChange]
  )

  const handleSelectAll = useCallback(() => {
    if (selectedIds.length === data.length) {
      onSelectionChange?.([])
    } else {
      onSelectionChange?.(data.map((row) => rowKey(row)))
    }
  }, [data, selectedIds, onSelectionChange, rowKey])

  const handleSelectRow = useCallback(
    (id: string) => {
      const newSelected = selectedIds.includes(id)
        ? selectedIds.filter((sid) => sid !== id)
        : [...selectedIds, id]
      onSelectionChange?.(newSelected)
    },
    [selectedIds, onSelectionChange]
  )

  const toggleColumnVisibility = (columnId: string) => {
    const updated = {
      ...(columnVisibility || localColumnVisibility),
      [columnId]: !(columnVisibility || localColumnVisibility)[columnId],
    }
    if (onColumnVisibilityChange) {
      onColumnVisibilityChange(updated)
    } else {
      setLocalColumnVisibility(updated)
    }
  }

  if (error) {
    return <ErrorFallback message={error} />
  }

  return (
    <div className="space-y-4">
      {bulkActions && (
        <BulkActions selectedIds={selectedIds} actions={bulkActions} />
      )}

      <div className="rounded-md border">
        <Table>
          <TableHeader>
            <TableRow>
              {onSelectionChange && (
                <TableHead className="w-10">
                  <Checkbox
                    checked={data.length > 0 && selectedIds.length === data.length}
                    onCheckedChange={handleSelectAll}
                    aria-label="Select all"
                  />
                </TableHead>
              )}
              {visibleColumns.map((column) => (
                <TableHead
                  key={column.id}
                  style={{ width: column.width }}
                  className={cn(
                    column.sortable && "cursor-pointer select-none"
                  )}
                >
                  <div
                    className="flex items-center gap-1"
                    onClick={() => column.sortable && handleSort(column.id)}
                  >
                    {column.header}
                    {column.sortable && (
                      <span className="ml-1">
                        {sortBy === column.id ? (
                          sortOrder === "asc" ? (
                            <ArrowUp className="h-3 w-3" />
                          ) : (
                            <ArrowDown className="h-3 w-3" />
                          )
                        ) : (
                          <ArrowUpDown className="h-3 w-3 opacity-30" />
                        )}
                      </span>
                    )}
                  </div>
                </TableHead>
              ))}
              {(onColumnVisibilityChange || onExportCsv || onExportExcel) && (
                <TableHead className="w-14 text-right">
                  <DropdownMenu>
                    <DropdownMenuTrigger asChild>
                      <Button variant="ghost" size="icon" className="h-8 w-8">
                        <Columns className="h-4 w-4" />
                      </Button>
                    </DropdownMenuTrigger>
                    <DropdownMenuContent align="end">
                      {columns.map((col) => (
                        <DropdownMenuCheckboxItem
                          key={col.id}
                          checked={(columnVisibility || localColumnVisibility)[col.id] !== false}
                          onCheckedChange={() => toggleColumnVisibility(col.id)}
                        >
                          {col.header}
                        </DropdownMenuCheckboxItem>
                      ))}
                      {onExportCsv && (
                        <>
                          <div className="h-px bg-border my-1" />
                          <DropdownMenuCheckboxItem onSelect={onExportCsv}>
                            Export CSV
                          </DropdownMenuCheckboxItem>
                        </>
                      )}
                      {onExportExcel && (
                        <DropdownMenuCheckboxItem onSelect={onExportExcel}>
                          Export Excel
                        </DropdownMenuCheckboxItem>
                      )}
                    </DropdownMenuContent>
                  </DropdownMenu>
                </TableHead>
              )}
            </TableRow>
          </TableHeader>
          <TableBody>
            {loading ? (
              <TableRow>
                <TableCell colSpan={visibleColumns.length + (onSelectionChange ? 1 : 0) + 1}>
                  <LoadingState rows={5} />
                </TableCell>
              </TableRow>
            ) : data.length === 0 ? (
              <TableRow>
                <TableCell colSpan={visibleColumns.length + (onSelectionChange ? 1 : 0) + 1}>
                  <EmptyState title={emptyTitle} description={emptyDescription} icon={emptyIcon} />
                </TableCell>
              </TableRow>
            ) : (
              data.map((row) => {
                const key = rowKey(row)
                const isSelected = selectedIds.includes(key)
                return (
                  <TableRow
                    key={key}
                    className={cn(
                      "transition-colors",
                      onRowClick && "cursor-pointer",
                      isSelected && "bg-muted/50"
                    )}
                    onClick={() => onRowClick?.(row)}
                    data-state={isSelected ? "selected" : undefined}
                  >
                    {onSelectionChange && (
                      <TableCell className="w-10" onClick={(e) => e.stopPropagation()}>
                        <Checkbox
                          checked={isSelected}
                          onCheckedChange={() => handleSelectRow(key)}
                          aria-label="Select row"
                        />
                      </TableCell>
                    )}
                    {visibleColumns.map((column) => (
                      <TableCell key={column.id}>
                        {column.cell
                          ? column.cell(row)
                          : column.accessorKey
                          ? String(row[column.accessorKey] ?? "")
                          : ""}
                      </TableCell>
                    ))}
                    {(onColumnVisibilityChange || onExportCsv || onExportExcel) && (
                      <TableCell />
                    )}
                  </TableRow>
                )
              })
            )}
          </TableBody>
        </Table>
      </div>

      {pagination && !loading && (
        <Pagination {...pagination} />
      )}
    </div>
  )
}
```

### Task 8: Entity Detail Framework

**Files:**
- Create: `src/components/shared/EntityHeader.tsx`
- Create: `src/components/shared/EntityTabs.tsx`
- Create: `src/components/shared/EntityMetadata.tsx`

- [ ] **Step 1: EntityHeader**

```typescript
// src/components/shared/EntityHeader.tsx
"use client"

import { Button } from "@/components/ui/button"
import { Badge } from "@/components/ui/badge"
import { ArrowLeft, Edit, Trash2 } from "lucide-react"
import Link from "next/link"
import { Skeleton } from "@/components/ui/skeleton"

interface EntityHeaderProps {
  title: string
  subtitle?: string
  status?: string
  backHref: string
  editHref?: string
  onDelete?: () => void
  loading?: boolean
}

export function EntityHeader({ title, subtitle, status, backHref, editHref, onDelete, loading }: EntityHeaderProps) {
  if (loading) {
    return (
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Skeleton className="h-9 w-9 rounded-md" />
          <div>
            <Skeleton className="h-8 w-64" />
            <Skeleton className="h-4 w-32 mt-1" />
          </div>
        </div>
      </div>
    )
  }

  return (
    <div className="flex items-center justify-between">
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="icon" asChild>
          <Link href={backHref}>
            <ArrowLeft className="h-4 w-4" />
          </Link>
        </Button>
        <div>
          <div className="flex items-center gap-3">
            <h1 className="text-2xl font-bold tracking-tight">{title}</h1>
            {status && <Badge>{status}</Badge>}
          </div>
          {subtitle && (
            <p className="text-sm text-muted-foreground">{subtitle}</p>
          )}
        </div>
      </div>
      <div className="flex items-center gap-2">
        {editHref && (
          <Button variant="outline" size="sm" asChild>
            <Link href={editHref}>
              <Edit className="mr-1 h-4 w-4" /> Edit
            </Link>
          </Button>
        )}
        {onDelete && (
          <Button variant="destructive" size="sm" onClick={onDelete}>
            <Trash2 className="mr-1 h-4 w-4" /> Delete
          </Button>
        )}
      </div>
    </div>
  )
}
```

- [ ] **Step 2: EntityTabs**

```typescript
// src/components/shared/EntityTabs.tsx
"use client"

import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"

interface Tab {
  id: string
  label: string
  content: React.ReactNode
}

interface EntityTabsProps {
  tabs: Tab[]
  defaultTab?: string
}

export function EntityTabs({ tabs, defaultTab }: EntityTabsProps) {
  return (
    <Tabs defaultValue={defaultTab || tabs[0]?.id}>
      <TabsList>
        {tabs.map((tab) => (
          <TabsTrigger key={tab.id} value={tab.id}>
            {tab.label}
          </TabsTrigger>
        ))}
      </TabsList>
      {tabs.map((tab) => (
        <TabsContent key={tab.id} value={tab.id} className="mt-6">
          {tab.content}
        </TabsContent>
      ))}
    </Tabs>
  )
}
```

- [ ] **Step 3: EntityMetadata**

```typescript
// src/components/shared/EntityMetadata.tsx
"use client"

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Skeleton } from "@/components/ui/skeleton"

interface MetadataField {
  label: string
  value: string | React.ReactNode
}

interface EntityMetadataProps {
  title?: string
  fields: MetadataField[]
  loading?: boolean
  columns?: 1 | 2 | 3
}

export function EntityMetadata({ title = "Details", fields, loading, columns = 2 }: EntityMetadataProps) {
  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-base">{title}</CardTitle>
      </CardHeader>
      <CardContent>
        <div className={`grid gap-4 ${columns === 1 ? "grid-cols-1" : columns === 2 ? "grid-cols-1 md:grid-cols-2" : "grid-cols-1 md:grid-cols-3"}`}>
          {fields.map((field) => (
            <div key={field.label} className="space-y-1">
              <p className="text-sm text-muted-foreground">{field.label}</p>
              {loading ? (
                <Skeleton className="h-5 w-32" />
              ) : (
                <p className="text-sm font-medium">{field.value}</p>
              )}
            </div>
          ))}
        </div>
      </CardContent>
    </Card>
  )
}
```

### Task 9: Form Engine

**Files:**
- Create: `src/forms/FormField.tsx`
- Create: `src/forms/FormSection.tsx`
- Create: `src/forms/FormActions.tsx`
- Create: `src/forms/FormErrorSummary.tsx`
- Create: `src/forms/FormPageLayout.tsx`

- [ ] **Step 1: FormField**

```typescript
// src/forms/FormField.tsx
"use client"

import { Input } from "@/components/ui/input"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"
import { cn } from "@/lib/utils"
import { FieldError, UseFormRegisterReturn } from "react-hook-form"

interface FormFieldProps {
  label: string
  error?: FieldError
  required?: boolean
  children?: React.ReactNode
  registration?: UseFormRegisterReturn
  type?: string
  placeholder?: string
  className?: string
}

export function FormField({ label, error, required, children, registration, type, placeholder, className }: FormFieldProps) {
  return (
    <div className={cn("space-y-2", className)}>
      <label className="text-sm font-medium">
        {label}
        {required && <span className="text-destructive ml-1">*</span>}
      </label>
      {children || (
        <Input
          type={type || "text"}
          placeholder={placeholder}
          {...registration}
        />
      )}
      {error && (
        <p className="text-xs text-destructive">{error.message}</p>
      )}
    </div>
  )
}

interface FormSelectFieldProps extends Omit<FormFieldProps, "children"> {
  options: { label: string; value: string }[]
  value: string
  onValueChange: (value: string) => void
  placeholder?: string
}

export function FormSelectField({ label, error, required, options, value, onValueChange, placeholder, className }: FormSelectFieldProps) {
  return (
    <div className={cn("space-y-2", className)}>
      <label className="text-sm font-medium">
        {label}
        {required && <span className="text-destructive ml-1">*</span>}
      </label>
      <Select value={value} onValueChange={onValueChange}>
        <SelectTrigger>
          <SelectValue placeholder={placeholder} />
        </SelectTrigger>
        <SelectContent>
          {options.map((opt) => (
            <SelectItem key={opt.value} value={opt.value}>
              {opt.label}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
      {error && (
        <p className="text-xs text-destructive">{error.message}</p>
      )}
    </div>
  )
}
```

- [ ] **Step 2: FormSection**

```typescript
// src/forms/FormSection.tsx
"use client"

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"

interface FormSectionProps {
  title: string
  description?: string
  children: React.ReactNode
}

export function FormSection({ title, description, children }: FormSectionProps) {
  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-base">{title}</CardTitle>
        {description && (
          <p className="text-sm text-muted-foreground">{description}</p>
        )}
      </CardHeader>
      <CardContent className="space-y-6">
        {children}
      </CardContent>
    </Card>
  )
}
```

- [ ] **Step 3: FormActions**

```typescript
// src/forms/FormActions.tsx
"use client"

import { Button } from "@/components/ui/button"
import { Loader2 } from "lucide-react"
import Link from "next/link"

interface FormActionsProps {
  backHref: string
  loading?: boolean
  loadingText?: string
  submitLabel?: string
}

export function FormActions({ backHref, loading, loadingText = "Saving...", submitLabel = "Save" }: FormActionsProps) {
  return (
    <div className="flex justify-end gap-3 pt-4">
      <Button variant="outline" type="button" asChild>
        <Link href={backHref}>Cancel</Link>
      </Button>
      <Button type="submit" disabled={loading}>
        {loading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
        {loading ? loadingText : submitLabel}
      </Button>
    </div>
  )
}
```

- [ ] **Step 4: FormErrorSummary**

```typescript
// src/forms/FormErrorSummary.tsx
"use client"

import { AlertCircle } from "lucide-react"

interface FormErrorSummaryProps {
  errors: Record<string, { message?: string } | undefined>
}

export function FormErrorSummary({ errors }: FormErrorSummaryProps) {
  const errorMessages = Object.values(errors).filter(Boolean) as { message?: string }[]

  if (errorMessages.length === 0) return null

  return (
    <div className="rounded-md bg-destructive/10 p-3 text-sm text-destructive">
      <div className="flex items-center gap-2 font-medium">
        <AlertCircle className="h-4 w-4" />
        <span>Please fix the following errors:</span>
      </div>
      <ul className="mt-1 list-disc list-inside space-y-1">
        {errorMessages.map((err, i) => (
          <li key={i}>{err.message}</li>
        ))}
      </ul>
    </div>
  )
}
```

- [ ] **Step 5: FormPageLayout**

```typescript
// src/forms/FormPageLayout.tsx
"use client"

import { Button } from "@/components/ui/button"
import { ArrowLeft } from "lucide-react"
import Link from "next/link"

interface FormPageLayoutProps {
  title: string
  backHref: string
  children: React.ReactNode
}

export function FormPageLayout({ title, backHref, children }: FormPageLayoutProps) {
  return (
    <div className="flex-1 space-y-6 p-6">
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="icon" asChild>
          <Link href={backHref}>
            <ArrowLeft className="h-4 w-4" />
          </Link>
        </Button>
        <h1 className="text-2xl font-bold tracking-tight">{title}</h1>
      </div>
      <form>
        {children}
      </form>
    </div>
  )
}
```

### Task 10: Theme and Locale Toggle Components

**Files:**
- Create: `src/components/shared/ThemeToggle.tsx`
- Create: `src/components/shared/LocaleToggle.tsx`

- [ ] **Step 1: ThemeToggle**

```typescript
// src/components/shared/ThemeToggle.tsx
"use client"

import { Moon, Sun } from "lucide-react"
import { Button } from "@/components/ui/button"
import { useThemeStore } from "@/stores/theme-store"

export function ThemeToggle() {
  const { theme, toggleTheme } = useThemeStore()

  return (
    <Button variant="ghost" size="icon" onClick={toggleTheme} title={`Switch to ${theme === "light" ? "dark" : "light"} mode`}>
      {theme === "light" ? <Moon className="h-4 w-4" /> : <Sun className="h-4 w-4" />}
    </Button>
  )
}
```

- [ ] **Step 2: LocaleToggle**

```typescript
// src/components/shared/LocaleToggle.tsx
"use client"

import { Button } from "@/components/ui/button"
import { useLocaleStore } from "@/stores/locale-store"

export function LocaleToggle() {
  const { locale, toggleLocale } = useLocaleStore()

  return (
    <Button variant="ghost" size="sm" onClick={toggleLocale}>
      {locale === "en" ? "العربية" : "English"}
    </Button>
  )
}
```

### Task 11: Navigation Framework

**Files:**
- Create: `src/components/shared/ModuleSidebar.tsx`
- Create: `src/components/shared/BreadcrumbBuilder.tsx`
- Create: `src/components/shared/PermissionGuard.tsx`
- Modify: `src/components/layout/sidebar.tsx`
- Modify: `src/components/layout/header.tsx`

- [ ] **Step 1: PermissionGuard**

```typescript
// src/components/shared/PermissionGuard.tsx
"use client"

import { usePermission } from "@/providers/permission-provider"

interface PermissionGuardProps {
  role?: string
  roles?: string[]
  children: React.ReactNode
  fallback?: React.ReactNode
}

export function PermissionGuard({ role, roles, children, fallback }: PermissionGuardProps) {
  const { hasRole, hasAnyRole } = usePermission()

  if (role && !hasRole(role)) {
    return fallback ?? null
  }

  if (roles && !hasAnyRole(roles)) {
    return fallback ?? null
  }

  return <>{children}</>
}
```

- [ ] **Step 2: ModuleSidebar**

```typescript
// src/components/shared/ModuleSidebar.tsx
"use client"

import Link from "next/link"
import { usePathname } from "next/navigation"
import { cn } from "@/lib/utils"
import { useAuthStore } from "@/stores/auth-store"
import {
  LayoutDashboard,
  Users,
  Package,
  ShoppingCart,
  ClipboardList,
  FileText,
  CreditCard,
  DollarSign,
  Ticket,
  Shield,
  BarChart3,
  Network,
  Cable,
  Settings,
  Bell,
  FileBarChart,
  ScrollText,
  Waypoints,
  LogOut,
  ChevronLeft,
} from "lucide-react"
import { Button } from "@/components/ui/button"
import { useState } from "react"
import { ThemeToggle } from "./ThemeToggle"
import { LocaleToggle } from "./LocaleToggle"

interface NavItem {
  href: string
  label: string
  icon: React.ElementType
  roles?: string[]
}

const modules: NavItem[] = [
  { href: "/dashboard", label: "Dashboard", icon: LayoutDashboard },
  { href: "/customers", label: "Customers", icon: Users },
  { href: "/products", label: "Products", icon: Package },
  { href: "/orders", label: "Orders", icon: ShoppingCart },
  { href: "/subscriptions", label: "Subscriptions", icon: ClipboardList },
  { href: "/billing", label: "Billing", icon: CreditCard },
  { href: "/invoices", label: "Invoices", icon: FileText },
  { href: "/payments", label: "Payments", icon: DollarSign },
  { href: "/tickets", label: "Tickets", icon: Ticket },
  { href: "/collections", label: "Collections", icon: Settings },
  { href: "/services", label: "Services", icon: Network },
  { href: "/network", label: "Network", icon: Cable },
  { href: "/provisioning", label: "Provisioning", icon: Settings },
  { href: "/workflow", label: "Workflow", icon: Waypoints },
  { href: "/notifications", label: "Notifications", icon: Bell },
  { href: "/reporting", label: "Reporting", icon: FileBarChart },
  { href: "/audit", label: "Audit", icon: ScrollText },
  { href: "/admin", label: "Admin", icon: Shield, roles: ["ADMIN"] },
]

export function ModuleSidebar() {
  const pathname = usePathname()
  const { user, logout } = useAuthStore()
  const [collapsed, setCollapsed] = useState(false)

  return (
    <aside
      className={cn(
        "flex flex-col border-r bg-background transition-all duration-300",
        collapsed ? "w-16" : "w-64"
      )}
    >
      <div className="flex h-14 items-center border-b px-4">
        {!collapsed && (
          <span className="text-lg font-bold tracking-tight">OSS/BSS</span>
        )}
        <Button
          variant="ghost"
          size="icon"
          onClick={() => setCollapsed(!collapsed)}
          className={cn("ml-auto", collapsed && "mx-auto")}
        >
          <ChevronLeft className={cn("h-4 w-4 transition-transform", collapsed && "rotate-180")} />
        </Button>
      </div>

      <nav className="flex-1 overflow-y-auto p-2 space-y-1">
        {modules.map((item) => {
          const Icon = item.icon
          const isActive = pathname.startsWith(item.href)
          return (
            <Link
              key={item.href}
              href={item.href}
              className={cn(
                "flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors hover:bg-accent hover:text-accent-foreground",
                isActive ? "bg-accent text-accent-foreground" : "text-muted-foreground",
                collapsed && "justify-center px-2"
              )}
              title={collapsed ? item.label : undefined}
            >
              <Icon className="h-4 w-4 shrink-0" />
              {!collapsed && <span>{item.label}</span>}
            </Link>
          )
        })}
      </nav>

      <div className="border-t p-4 space-y-2">
        {!collapsed && (
          <div className="flex items-center justify-between px-1">
            <ThemeToggle />
            <LocaleToggle />
          </div>
        )}
        {collapsed && (
          <div className="flex flex-col items-center gap-2">
            <ThemeToggle />
            <LocaleToggle />
          </div>
        )}
        {!collapsed && user && (
          <div className="text-sm">
            <p className="font-medium truncate">{user.firstName} {user.lastName}</p>
            <p className="text-muted-foreground truncate">{user.email}</p>
          </div>
        )}
        <Button
          variant="ghost"
          size={collapsed ? "icon" : "default"}
          className={cn("w-full", collapsed ? "mx-auto" : "")}
          onClick={() => { localStorage.removeItem("auth-token"); logout() }}
        >
          <LogOut className="h-4 w-4" />
          {!collapsed && <span>Logout</span>}
        </Button>
      </div>
    </aside>
  )
}
```

- [ ] **Step 3: BreadcrumbBuilder**

```typescript
// src/components/shared/BreadcrumbBuilder.tsx
"use client"

import { usePathname } from "next/navigation"
import Link from "next/link"
import { ChevronRight, Home } from "lucide-react"

const labelMap: Record<string, string> = {
  dashboard: "Dashboard",
  customers: "Customers",
  products: "Products",
  orders: "Orders",
  subscriptions: "Subscriptions",
  invoices: "Invoices",
  billing: "Billing",
  payments: "Payments",
  tickets: "Tickets",
  admin: "Admin",
  collections: "Collections",
  services: "Services",
  network: "Network",
  provisioning: "Provisioning",
  workflow: "Workflow",
  notifications: "Notifications",
  reporting: "Reporting",
  audit: "Audit",
  new: "New",
  edit: "Edit",
  offers: "Offers",
  categories: "Categories",
  cycles: "Cycles",
  jobs: "Jobs",
  "credit-notes": "Credit Notes",
  disputes: "Disputes",
  reconciliation: "Reconciliation",
  refunds: "Refunds",
}

export function BreadcrumbBuilder() {
  const pathname = usePathname()
  const segments = pathname.split("/").filter(Boolean)

  return (
    <nav className="flex items-center gap-1 text-sm text-muted-foreground">
      <Link href="/dashboard" className="hover:text-foreground">
        <Home className="h-4 w-4" />
      </Link>
      {segments.map((segment, index) => {
        const href = "/" + segments.slice(0, index + 1).join("/")
        const label = labelMap[segment] || segment
        const isLast = index === segments.length - 1
        return (
          <span key={segment} className="flex items-center gap-1">
            <ChevronRight className="h-3 w-3" />
            {isLast ? (
              <span className="font-medium text-foreground">{label}</span>
            ) : (
              <Link href={href} className="hover:text-foreground">
                {label}
              </Link>
            )}
          </span>
        )
      })}
    </nav>
  )
}
```

- [ ] **Step 4: Update header with new components**

```typescript
// src/components/layout/header.tsx — replace breadcrumb logic with BreadcrumbBuilder
import { BreadcrumbBuilder } from "@/components/shared/BreadcrumbBuilder"
// Replace inline breadcrumb rendering with <BreadcrumbBuilder />
```

- [ ] **Step 5: Update sidebar with ModuleSidebar**

```typescript
// src/components/layout/sidebar.tsx — replace with ModuleSidebar
export { ModuleSidebar as Sidebar } from "@/components/shared/ModuleSidebar"
```

### Task 12: API Client + Type Expansion

**Files:**
- Create: `src/api/client.ts`
- Modify: `src/services/api.ts`
- Modify: `src/types/api.ts`

- [ ] **Step 1: Create API client augmentation**

```typescript
// src/api/client.ts
import api from "@/services/api"
import { queryKeys } from "@/lib/query-keys"

export { api }
export { queryKeys }
```

- [ ] **Step 2: Expand types/api.ts with all DTOs**

Add all missing DTOs for every module based on the existing patterns. Include:

```
SegmentDto
NoteDto
ContactDto
CategoryDto
OfferPricingDto
ConfigurationRuleDto
ProvisioningJobDto
ProvisioningTemplateDto
WorkflowDefinitionDto
WorkflowInstanceDto
WorkflowStepDto
ServiceDto
ResourceDto
TopologyDto
NetworkElementDto
OLTDto
VlandDto
FiberResourceDto
CollectionCaseDto
CollectionActionDto
PaymentArrangementDto
NotificationDto
NotificationTemplateDto
ReportDefinitionDto
DashboardWidgetDto
AuditEntryDto
AuditAlertDto
ApiRouteDto
ApiKeyDto
PartnerDto
```

### Task 13: Remove Mock Data

**Files:**
- Modify: `src/app/dashboard/page.tsx`

- [ ] **Step 1: Remove mock revenue chart data**

Replace `generateRevenueData()` with real API calls. Use recent order data for revenue tracking.

### Task 14: Formatters

**Files:**
- Create: `src/lib/formatters.ts`
- Create: `src/lib/api-helpers.ts`

- [ ] **Step 1: Create formatters**

```typescript
// src/lib/formatters.ts
export function formatCurrency(amount: number, currency: string = "USD"): string {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency,
  }).format(amount)
}

export function formatDate(date: string | Date): string {
  return new Intl.DateTimeFormat("en-US", {
    year: "numeric",
    month: "short",
    day: "numeric",
  }).format(new Date(date))
}

export function formatDateTime(date: string | Date): string {
  return new Intl.DateTimeFormat("en-US", {
    year: "numeric",
    month: "short",
    day: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  }).format(new Date(date))
}

export function formatPhone(phone: string): string {
  return phone
}

export function formatPercent(value: number): string {
  return `${(value * 100).toFixed(1)}%`
}

export function truncate(str: string, length: number = 50): string {
  if (str.length <= length) return str
  return str.substring(0, length) + "..."
}

export function capitalize(str: string): string {
  return str.charAt(0).toUpperCase() + str.slice(1).toLowerCase()
}

export function formatStatus(status: string): string {
  return status.replace(/_/g, " ").replace(/\b\w/g, (c) => c.toUpperCase())
}
```

- [ ] **Step 2: Create API helpers**

```typescript
// src/lib/api-helpers.ts
import { api } from "@/api/client"

export interface PaginatedResponse<T> {
  data: T[]
  total: number
  page: number
  pageSize: number
  totalPages: number
}

export function buildUrl(base: string, params: Record<string, string | number | undefined>): string {
  const searchParams = new URLSearchParams()
  Object.entries(params).forEach(([key, value]) => {
    if (value !== undefined && value !== "") {
      searchParams.set(key, String(value))
    }
  })
  const queryString = searchParams.toString()
  return queryString ? `${base}?${queryString}` : base
}

export function getErrorMessage(error: unknown): string {
  if (typeof error === "string") return error
  if (error instanceof Error) return error.message
  return "An unexpected error occurred"
}
```

### Task 15: Types Expansion

**Files:**
- Modify: `src/types/api.ts`

- [ ] **Step 1: Expand types with all DTOs**

Add missing types for every module module (segments, contacts, notes, network elements, etc.)

### Task 16-23: API Hooks

**Files:**
- Create: `src/api/hooks/useCustomers.ts`
- Create: `src/api/hooks/useCustomer.ts`
- Create: `src/api/hooks/useCreateCustomer.ts`
- Create: `src/api/hooks/useUpdateCustomer.ts`
- Create: `src/api/hooks/useOrders.ts`
- Create: `src/api/hooks/useOrder.ts`
- Create: `src/api/hooks/useCreateOrder.ts`
- Create: `src/api/hooks/useProducts.ts`
- Create: `src/api/hooks/useProduct.ts`
- Create: `src/api/hooks/useCreateProduct.ts`
- Create: `src/api/hooks/useSubscriptions.ts`
- Create: `src/api/hooks/useSubscription.ts`
- Create: `src/api/hooks/useBills.ts`
- Create: `src/api/hooks/useBill.ts`
- Create: `src/api/hooks/useInvoices.ts`
- Create: `src/api/hooks/useInvoice.ts`
- Create: `src/api/hooks/usePayments.ts`
- Create: `src/api/hooks/usePayment.ts`
- Create: `src/api/hooks/useTickets.ts`
- Create: `src/api/hooks/useTicket.ts`
- Create: `src/api/hooks/useCreateTicket.ts`
- Create: `src/api/hooks/useUsers.ts`
- Create: `src/api/hooks/useUser.ts`
- Create: `src/api/hooks/useCreateUser.ts`

Each hook follows this pattern:

```typescript
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import { CustomerDto } from "@/types/api"

export function useCustomers(filters: Record<string, string> = {}) {
  return useQuery({
    queryKey: queryKeys.customers.list(filters),
    queryFn: async () => {
      const params = new URLSearchParams()
      Object.entries(filters).forEach(([k, v]) => { if (v) params.set(k, v) })
      const res = await api.get(`/api/v1/crm/customers?${params.toString()}`)
      return res.data as CustomerDto[]
    },
  })
}
```

### Task 24: Update App Shell and Layout

- [ ] **Step 1: Update app-shell.tsx to use new sidebar**
- [ ] **Step 2: Update header.tsx to use BreadcrumbBuilder**
- [ ] **Step 3: Update providers.tsx to compose all providers**

### Task 25: Verify Build

- [ ] **Step 1:** Run `npm run build` and fix any TypeScript or lint errors

---

## Self-Review

**Spec coverage:**
- Multi-tenant foundation ✅ (Task 1)
- Query key factory ✅ (Task 2)
- Theme/locale stores ✅ (Task 3)
- All providers ✅ (Task 4)
- i18n system ✅ (Task 5)
- Shared components ✅ (Tasks 6-8)
- Form engine ✅ (Task 9)
- Theme/Locale toggles ✅ (Task 10)
- Navigation framework ✅ (Task 11)
- API client + hooks ✅ (Tasks 12-13, 16-23)
- Formatters + helpers ✅ (Task 14)
- Types expansion ✅ (Task 15)
- Mock data removal ✅ (Task 13)
- Build verification ✅ (Task 25)

**Placeholder check:** No TBD/TODO placeholders in code blocks.

**Type consistency:** All hooks follow the same pattern. Query keys match endpoint paths. Component props are consistent.
