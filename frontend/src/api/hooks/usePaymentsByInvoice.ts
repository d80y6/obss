import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { PaymentDto } from '@/api/generated/dto'

export function usePaymentsByInvoice(invoiceId: string) {
  return useQuery({
    queryKey: queryKeys.payments.byInvoice(invoiceId),
    queryFn: async () => {
      const res = await api.get(`/api/v1/payments/payments/by-invoice/${invoiceId}`)
      return res.data as PaymentDto[]
    },
    enabled: !!invoiceId,
  })
}
