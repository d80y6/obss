import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { ReconciliationDto } from '@/api/generated/dto'

export function useReconciliation() {
  return useQuery({
    queryKey: queryKeys.payments.reconciliation.list({}),
    queryFn: async () => {
      const res = await api.get("/api/v1/payments/reconciliation")
      return res.data as ReconciliationDto
    },
  })
}
