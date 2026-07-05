import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"

export function useOrderTimeline(orderId: string) {
  return useQuery({
    queryKey: queryKeys.orders.timeline(orderId),
    queryFn: async () => {
      const res = await api.get(`/api/v1/orders/orders/${orderId}/timeline`)
      return res.data
    },
    enabled: !!orderId,
  })
}
