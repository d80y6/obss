import { useQuery } from "@tanstack/react-query"
import api from "@/services/api"
import { queryKeys } from "@/lib/query-keys"
import type { NasDto } from "@/api/generated/dto"

interface PaginatedNasResult {
  items: NasDto[]
  totalCount: number
}

export function useNasDevices(filters: Record<string, string> = {}) {
  return useQuery({
    queryKey: queryKeys.aaa.nas.list(filters),
    queryFn: async () => {
      const params = new URLSearchParams()
      Object.entries(filters).forEach(([k, v]) => { if (v) params.set(k, v) })
      const res = await api.get(`/api/v1/aaa/nas?${params.toString()}`)
      return res.data as PaginatedNasResult
    },
  })
}
