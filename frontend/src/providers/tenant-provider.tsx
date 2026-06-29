"use client"

import { createContext, useContext, useEffect } from "react"
import { useTenantStore, Tenant } from "@/stores/tenant-store"
import { useAuthStore } from "@/stores/auth-store"
import { useQuery } from "@tanstack/react-query"
import api from "@/services/api"

const TenantContext = createContext<Tenant | null>(null)

export function TenantProvider({ children }: { children: React.ReactNode }) {
  const { tenant, setTenant, setTenants } = useTenantStore()
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated)

  const token = typeof window !== "undefined" ? localStorage.getItem("auth-token") : null

  const { data: tenants } = useQuery({
    queryKey: ["tenants"],
    queryFn: async () => {
      const res = await api.get("/api/v1/iam/tenants")
      return res.data as Tenant[]
    },
    enabled: !tenant && isAuthenticated && !!token,
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
