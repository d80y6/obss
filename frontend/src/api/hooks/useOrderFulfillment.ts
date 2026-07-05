import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { OrderFulfillmentDto } from '@/api/generated/dto'

export function useOrderFulfillment(orderId: string) {
  return useQuery({
    queryKey: queryKeys.orders.fulfillment(orderId),
    queryFn: async () => {
      const res = await api.get(`/api/v1/orders/orders/${orderId}/fulfillment`)
      return res.data as OrderFulfillmentDto
    },
    enabled: !!orderId,
  })
}
