"use client"

import { useTenantStore, Tenant } from "@/stores/tenant-store"
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
