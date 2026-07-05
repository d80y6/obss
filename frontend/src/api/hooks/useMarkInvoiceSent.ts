import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"

export function useMarkInvoiceSent() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      const res = await api.post(`/api/v1/invoices/invoices/${id}/send`)
      return res.data
    },
    onSuccess: (_data, id) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.invoices.detail(id) })
      queryClient.invalidateQueries({ queryKey: queryKeys.invoices.lists() })
    },
  })
}
