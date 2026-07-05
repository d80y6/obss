import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { RecordInvoicePaymentCommand } from '@/api/generated'

export function useRecordInvoicePayment(id: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: RecordInvoicePaymentCommand) => {
      const res = await api.post(`/api/v1/invoices/invoices/${id}/pay`, data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.invoices.detail(id) })
      queryClient.invalidateQueries({ queryKey: queryKeys.invoices.lists() })
    },
  })
}
