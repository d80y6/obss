import { useQuery } from "@tanstack/react-query"
import api from "@/services/api"
import { queryKeys } from "@/lib/query-keys"
import type { RadiusSessionDto } from "@/api/generated/dto"

interface PaginatedSessionsResult {
  items: RadiusSessionDto[]
  totalCount: number
}

export function useSessions(filters: Record<string, string> = {}) {
  return useQuery({
    queryKey: queryKeys.aaa.sessions.list(filters),
    queryFn: async () => {
      const params = new URLSearchParams()
      Object.entries(filters).forEach(([k, v]) => { if (v) params.set(k, v) })
      const res = await api.get(`/api/v1/aaa/sessions?${params.toString()}`)
      return res.data as PaginatedSessionsResult
    },
  })
}
