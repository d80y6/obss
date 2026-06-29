import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { PaymentDto } from '@/api/generated/dto'

export function usePayment(id: string) {
  return useQuery({
    queryKey: queryKeys.payments.detail(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/payments/payments/${id}`)
      return res.data as PaymentDto
    },
    enabled: !!id,
  })
}
