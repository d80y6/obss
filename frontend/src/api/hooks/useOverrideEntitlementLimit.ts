import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"

export function useOverrideEntitlementLimit(id: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: Record<string, unknown>) => {
      const res = await api.post(`/api/v1/subscriptions/subscriptions/${id}/entitlements/override`, data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.subscriptions.entitlements(id) })
      queryClient.invalidateQueries({ queryKey: queryKeys.subscriptions.detail(id) })
    },
  })
}
