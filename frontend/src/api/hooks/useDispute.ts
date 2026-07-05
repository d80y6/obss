import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { DisputeDto } from '@/api/generated/dto'

export function useDispute(id: string) {
  return useQuery({
    queryKey: [...queryKeys.invoices.disputes.all, "detail", id],
    queryFn: async () => {
      const res = await api.get(`/api/v1/invoices/invoices/disputes/${id}`)
      return res.data as DisputeDto
    },
    enabled: !!id,
  })
}
