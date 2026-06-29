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
