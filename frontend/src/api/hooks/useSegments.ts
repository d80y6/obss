import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { SegmentDto } from '@/api/generated/dto'

export function useSegments(filters: Record<string, string> = {}) {
  return useQuery({
    queryKey: queryKeys.segments.list(filters),
    queryFn: async () => {
      const params = new URLSearchParams()
      Object.entries(filters).forEach(([k, v]) => { if (v) params.set(k, v) })
      const res = await api.get(`/api/v1/crm/segments?${params.toString()}`)
      const total = res.headers['x-total-count'] ? parseInt(res.headers['x-total-count'], 10) : null
      return { items: res.data as SegmentDto[], total }
    },
  })
}
