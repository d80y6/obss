import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"

export function useExtendSubscriptionEndDate(id: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: Record<string, unknown>) => {
      const res = await api.put(`/api/v1/subscriptions/subscriptions/${id}/end-date`, data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.subscriptions.detail(id) })
      queryClient.invalidateQueries({ queryKey: queryKeys.subscriptions.lists() })
    },
  })
}
