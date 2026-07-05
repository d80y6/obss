import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { ReconciliationDto } from '@/api/generated/dto'

export function useReconciliationDetail(id: string) {
  return useQuery({
    queryKey: queryKeys.payments.reconciliation.detail(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/payments/payments/reconciliation/${id}`)
      return res.data as ReconciliationDto
    },
    enabled: !!id,
  })
}
