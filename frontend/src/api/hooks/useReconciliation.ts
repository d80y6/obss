import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { ReconciliationDto } from '@/api/generated/dto'

export function useReconciliation(filters: Record<string, string> = {}) {
  return useQuery({
    queryKey: queryKeys.payments.reconciliation.list(filters),
    queryFn: async () => {
      const params = new URLSearchParams()
      Object.entries(filters).forEach(([k, v]) => { if (v) params.set(k, v) })
      const res = await api.get(`/api/v1/payments/payments/reconciliation?${params.toString()}`)
      return res.data as ReconciliationDto[]
    },
  })
}
