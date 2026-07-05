import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { CreateSubscriptionCommand } from '@/api/generated'

export function useCreateSubscription() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: CreateSubscriptionCommand) => {
      const res = await api.post("/api/v1/subscriptions/subscriptions", data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.subscriptions.lists() })
    },
  })
}
