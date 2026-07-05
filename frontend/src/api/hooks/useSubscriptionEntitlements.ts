import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { EntitlementDto } from '@/api/generated/dto'

export function useSubscriptionEntitlements(id: string) {
  return useQuery({
    queryKey: queryKeys.subscriptions.entitlements(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/subscriptions/subscriptions/${id}/entitlements`)
      return res.data as EntitlementDto[]
    },
    enabled: !!id,
  })
}
