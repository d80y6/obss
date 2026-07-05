import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { PaymentGatewayInfo } from '@/api/generated/dto'

export function usePaymentGateways() {
  return useQuery({
    queryKey: queryKeys.payments.gateways.list({}),
    queryFn: async () => {
      const res = await api.get("/api/v1/payments/payments/gateways")
      return res.data as PaymentGatewayInfo[]
    },
  })
}
