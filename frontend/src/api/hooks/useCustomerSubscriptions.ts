import { useQuery } from "@tanstack/react-query"
import { api } from "@/api/client"
import type { SubscriptionSummaryDto } from '@/api/generated/dto'

export function useCustomerSubscriptions(customerId: string) {
  return useQuery({
    queryKey: ["customers", customerId, "subscriptions"],
    queryFn: async () => {
      const res = await api.get(`/api/v1/subscriptions/customers/${customerId}/subscriptions`)
      return res.data as SubscriptionSummaryDto[]
    },
    enabled: !!customerId,
  })
}
