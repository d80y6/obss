import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"

export function useActivateSubscription() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      const res = await api.post(`/api/v1/subscriptions/subscriptions/${id}/activate`)
      return res.data
    },
    onSuccess: (_data, id) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.subscriptions.detail(id) })
      queryClient.invalidateQueries({ queryKey: queryKeys.subscriptions.lists() })
    },
  })
}
