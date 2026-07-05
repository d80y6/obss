import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { RejectDisputeCommand } from '@/api/generated'

export function useRejectDispute(id: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: RejectDisputeCommand) => {
      const res = await api.post(`/api/v1/invoices/invoices/disputes/${id}/reject`, data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.invoices.disputes.all })
    },
  })
}
