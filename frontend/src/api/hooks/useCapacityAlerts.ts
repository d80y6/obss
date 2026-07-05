import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { CapacityAlertDto } from '@/api/generated/dto'

export function useCapacityAlerts() {
  return useQuery({
    queryKey: queryKeys.networks.capacity.alerts.list(),
    queryFn: async () => {
      const res = await api.get("/api/v1/network/capacity/alerts")
      return res.data as CapacityAlertDto[]
    },
  })
}
