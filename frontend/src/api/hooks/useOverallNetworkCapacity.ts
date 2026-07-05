import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { CapacityOverviewDto } from '@/api/generated/dto'

export function useOverallNetworkCapacity() {
  return useQuery({
    queryKey: queryKeys.networks.capacity.overview(),
    queryFn: async () => {
      const res = await api.get("/api/v1/network/capacity/overview")
      return res.data as CapacityOverviewDto
    },
  })
}
