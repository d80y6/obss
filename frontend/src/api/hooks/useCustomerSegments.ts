import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { SegmentDto } from '@/api/generated/dto'

export function useCustomerSegments(customerId: string) {
  return useQuery({
    queryKey: queryKeys.customers.segments(customerId),
    queryFn: async () => {
      const res = await api.get(`/api/v1/crm/customers/${customerId}/segments`)
      return res.data as SegmentDto[]
    },
    enabled: !!customerId,
  })
}
