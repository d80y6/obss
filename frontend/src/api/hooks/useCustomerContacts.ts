import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { ContactDto } from '@/api/generated/dto'

export function useCustomerContacts(customerId: string) {
  return useQuery({
    queryKey: queryKeys.customers.contacts(customerId),
    queryFn: async () => {
      const res = await api.get(`/api/v1/crm/customers/${customerId}/contacts`)
      return res.data as ContactDto[]
    },
    enabled: !!customerId,
  })
}
