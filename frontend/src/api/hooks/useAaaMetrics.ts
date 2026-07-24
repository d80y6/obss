import { useQuery } from "@tanstack/react-query"
import api from "@/services/api"
import { queryKeys } from "@/lib/query-keys"
import type { AaaMetricsDto } from "@/api/generated/dto"

export function useAaaMetrics() {
  return useQuery({
    queryKey: queryKeys.aaa.metrics(),
    queryFn: async () => {
      const res = await api.get("/api/v1/aaa/metrics")
      return res.data as AaaMetricsDto
    },
  })
}
