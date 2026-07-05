import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { ReconciliationItemDto } from '@/api/generated/dto'

export function useUnmatchedTransactions() {
  return useQuery({
    queryKey: queryKeys.payments.unmatched,
    queryFn: async () => {
      const res = await api.get("/api/v1/payments/payments/reconciliation/unmatched")
      return res.data as ReconciliationItemDto[]
    },
  })
}
