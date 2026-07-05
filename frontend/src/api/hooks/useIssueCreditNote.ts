import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { IssueCreditNoteCommand } from '@/api/generated'

export function useIssueCreditNote(id: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: IssueCreditNoteCommand) => {
      const res = await api.post(`/api/v1/invoices/invoices/${id}/credit-note`, data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.invoices.detail(id) })
      queryClient.invalidateQueries({ queryKey: queryKeys.invoices.lists() })
      queryClient.invalidateQueries({ queryKey: queryKeys.invoices.creditNotes.all })
    },
  })
}
