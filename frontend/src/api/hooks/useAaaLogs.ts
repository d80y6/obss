import { useQuery } from "@tanstack/react-query"
import api from "@/services/api"
import { queryKeys } from "@/lib/query-keys"
import type { AaaAuditLogDto } from "@/api/generated/dto"

interface PaginatedLogsResult {
  items: AaaAuditLogDto[]
  totalCount: number
}

export function useAaaLogs(filters: Record<string, string> = {}) {
  return useQuery({
    queryKey: queryKeys.aaa.logs.list(filters),
    queryFn: async () => {
      const params = new URLSearchParams()
      Object.entries(filters).forEach(([k, v]) => { if (v) params.set(k, v) })
      const res = await api.get(`/api/v1/aaa/logs?${params.toString()}`)
      return res.data as PaginatedLogsResult
    },
  })
}
