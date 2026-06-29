import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { CustomerDto } from '@/api/generated/dto'

export function useCustomer(id: string) {
  return useQuery({
    queryKey: queryKeys.customers.detail(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/crm/customers/${id}`)
      return res.data as CustomerDto
    },
    enabled: !!id,
  })
}
