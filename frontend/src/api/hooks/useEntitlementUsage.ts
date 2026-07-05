import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { EntitlementUsageDto } from '@/api/generated/dto'

export function useEntitlementUsage(id: string, type: string) {
  return useQuery({
    queryKey: [...queryKeys.subscriptions.usage(id), type],
    queryFn: async () => {
      const res = await api.get(`/api/v1/subscriptions/subscriptions/${id}/entitlements/usage?type=${type}`)
      return res.data as EntitlementUsageDto
    },
    enabled: !!id && !!type,
  })
}
