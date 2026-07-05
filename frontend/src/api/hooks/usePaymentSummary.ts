import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { PaymentSummaryDto } from '@/api/generated/dto'

export function usePaymentSummary() {
  return useQuery({
    queryKey: queryKeys.payments.summary,
    queryFn: async () => {
      const res = await api.get("/api/v1/payments/payments/summary")
      return res.data as PaymentSummaryDto
    },
  })
}
