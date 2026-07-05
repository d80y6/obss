import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { CancelSubscriptionCommand } from '@/api/generated'

export function useCancelSubscription(id: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: CancelSubscriptionCommand) => {
      const res = await api.post(`/api/v1/subscriptions/subscriptions/${id}/cancel`, data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.subscriptions.detail(id) })
      queryClient.invalidateQueries({ queryKey: queryKeys.subscriptions.lists() })
    },
  })
}
