import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"

interface AllocatePaymentCommand {
  paymentId: string
  invoiceId: string
  amount: number
}

export function useAllocatePayment(paymentId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: AllocatePaymentCommand) => {
      const res = await api.post(`/api/v1/payments/payments/${paymentId}/allocate`, data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.payments.detail(paymentId) })
      queryClient.invalidateQueries({ queryKey: queryKeys.payments.all })
    },
  })
}
