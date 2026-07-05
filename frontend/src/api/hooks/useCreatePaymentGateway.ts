import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { PaymentGatewayDto } from '@/api/generated/dto'

interface RegisterPaymentGatewayCommand {
  name: string
  provider: string
  configuration: string
  supportedCurrencies: string[]
  minAmount: number | null
  maxAmount: number | null
  transactionFee: number
  feeType: string
}

export function useCreatePaymentGateway() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: RegisterPaymentGatewayCommand) => {
      const res = await api.post<PaymentGatewayDto>("/api/v1/payments/payments/gateways", data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.payments.gateways.all })
    },
  })
}
