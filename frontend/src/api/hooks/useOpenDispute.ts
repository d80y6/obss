import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { OpenDisputeCommand } from '@/api/generated'

export function useOpenDispute(id: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: OpenDisputeCommand) => {
      const res = await api.post(`/api/v1/invoices/invoices/${id}/disputes`, data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.invoices.detail(id) })
      queryClient.invalidateQueries({ queryKey: queryKeys.invoices.lists() })
      queryClient.invalidateQueries({ queryKey: queryKeys.invoices.disputes.all })
    },
  })
}
