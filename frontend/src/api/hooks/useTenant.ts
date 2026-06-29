import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { TenantDto } from '@/api/generated/dto'

export function useTenant(id: string) {
  return useQuery({
    queryKey: queryKeys.tenants.detail(id),
    queryFn: async () => {
      const res = await api.get("/api/v1/iam/tenants")
      const tenants = res.data as TenantDto[]
      return tenants.find((t) => t.id === id) ?? null
    },
    enabled: !!id,
  })
}
