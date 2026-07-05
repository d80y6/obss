import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { RefundDto } from '@/api/generated/dto'

export function useRefunds() {
  return useQuery({
    queryKey: queryKeys.payments.refunds.list({}),
    queryFn: async () => {
      const res = await api.get("/api/v1/payments/refunds")
      return res.data as RefundDto[]
    },
  })
}
