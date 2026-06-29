import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { RecordPaymentCommand, PaymentDto } from "@/api/generated"

export function useCreatePayment() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: RecordPaymentCommand) => {
      const res = await api.post<PaymentDto>("/api/v1/payments/payments", data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.payments.all })
    },
  })
}
