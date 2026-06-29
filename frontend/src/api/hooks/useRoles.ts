import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { RoleDto } from '@/api/generated/dto'

export function useRoles(filters: Record<string, string> = {}) {
  return useQuery({
    queryKey: queryKeys.roles.list(filters),
    queryFn: async () => {
      const params = new URLSearchParams()
      Object.entries(filters).forEach(([k, v]) => { if (v) params.set(k, v) })
      const res = await api.get(`/api/v1/iam/roles?${params.toString()}`)
      return res.data as RoleDto[]
    },
  })
}
