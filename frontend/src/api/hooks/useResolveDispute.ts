import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { ResolveDisputeCommand } from '@/api/generated'

export function useResolveDispute(id: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: ResolveDisputeCommand) => {
      const res = await api.post(`/api/v1/invoices/invoices/disputes/${id}/resolve`, data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.invoices.disputes.all })
    },
  })
}
