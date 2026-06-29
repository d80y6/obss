import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { TenantDto } from '@/api/generated/dto'

export function useTenants(filters: Record<string, string> = {}) {
  return useQuery({
    queryKey: queryKeys.tenants.list(filters),
    queryFn: async () => {
      const params = new URLSearchParams()
      Object.entries(filters).forEach(([k, v]) => { if (v) params.set(k, v) })
      const res = await api.get(`/api/v1/iam/tenants?${params.toString()}`)
      return res.data as TenantDto[]
    },
  })
}
