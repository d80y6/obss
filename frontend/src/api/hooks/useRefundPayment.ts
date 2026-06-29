import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { RefundPaymentCommand } from "@/api/generated"

export function useRefundPayment(paymentId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: RefundPaymentCommand) => {
      const res = await api.post(`/api/v1/payments/payments/${paymentId}/refund`, data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.payments.detail(paymentId) })
      queryClient.invalidateQueries({ queryKey: queryKeys.payments.all })
      queryClient.invalidateQueries({ queryKey: queryKeys.payments.refunds.all })
    },
  })
}
