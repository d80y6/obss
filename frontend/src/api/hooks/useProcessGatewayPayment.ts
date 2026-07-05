import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { PaymentDto } from '@/api/generated/dto'

interface ProcessGatewayPaymentCommand {
  amount: number
  currency: string
  paymentMethod: string
  returnUrl?: string | null
  cancelUrl?: string | null
  customerId: string
  description?: string | null
}

export function useProcessGatewayPayment() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: ProcessGatewayPaymentCommand) => {
      const res = await api.post<PaymentDto>("/api/v1/payments/payments/process", data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.payments.all })
      queryClient.invalidateQueries({ queryKey: queryKeys.payments.summary })
    },
  })
}
