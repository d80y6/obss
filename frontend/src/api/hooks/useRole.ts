import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { RoleDto } from '@/api/generated/dto'

export function useRole(id: string) {
  return useQuery({
    queryKey: queryKeys.roles.detail(id),
    queryFn: async () => {
      const res = await api.get("/api/v1/iam/roles")
      const roles = res.data as RoleDto[]
      return roles.find((r) => r.id === id) ?? null
    },
    enabled: !!id,
  })
}
