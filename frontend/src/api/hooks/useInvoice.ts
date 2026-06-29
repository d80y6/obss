import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { InvoiceDto } from '@/api/generated/dto'

export function useInvoice(id: string) {
  return useQuery({
    queryKey: queryKeys.invoices.detail(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/invoices/invoices/${id}`)
      return res.data as InvoiceDto
    },
    enabled: !!id,
  })
}
