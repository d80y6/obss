import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { BillDto } from '@/api/generated/dto'

export function useBill(id: string) {
  return useQuery({
    queryKey: queryKeys.billing.bills.detail(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/billing/bills/${id}`)
      return res.data as BillDto
    },
    enabled: !!id,
  })
}
