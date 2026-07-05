import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { OrderFulfillmentDto } from '@/api/generated/dto'

/**
 * Fetches the fulfillment status for an order.
 * Uses the fulfillment endpoint since no dedicated timeline endpoint exists.
 */
export function useOrderTimeline(orderId: string) {
  return useQuery({
    queryKey: queryKeys.orders.timeline(orderId),
    queryFn: async () => {
      const res = await api.get(`/api/v1/orders/orders/${orderId}/fulfillment`)
      return res.data as OrderFulfillmentDto
    },
    enabled: !!orderId,
  })
}
