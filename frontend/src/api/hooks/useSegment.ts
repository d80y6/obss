import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { SegmentDto } from '@/api/generated/dto'

export function useSegment(id: string) {
  return useQuery({
    queryKey: queryKeys.segments.detail(id),
    queryFn: async () => {
      const res = await api.get("/api/v1/crm/segments")
      const items = res.data as SegmentDto[]
      return items.find((s) => s.id === id) ?? null
    },
    enabled: !!id,
  })
}
