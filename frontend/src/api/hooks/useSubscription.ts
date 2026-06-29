import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { SubscriptionDto } from '@/api/generated/dto'

export function useSubscription(id: string) {
  return useQuery({
    queryKey: queryKeys.subscriptions.detail(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/subscriptions/subscriptions/${id}`)
      return res.data as SubscriptionDto
    },
    enabled: !!id,
  })
}
