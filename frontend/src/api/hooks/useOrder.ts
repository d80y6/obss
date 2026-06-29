import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { OrderDto } from '@/api/generated/dto'

export function useOrder(id: string) {
  return useQuery({
    queryKey: queryKeys.orders.detail(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/orders/orders/${id}`)
      return res.data as OrderDto
    },
    enabled: !!id,
  })
}
