import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { NoteDto } from '@/api/generated/dto'

export function useCustomerNotes(customerId: string) {
  return useQuery({
    queryKey: queryKeys.customers.notes(customerId),
    queryFn: async () => {
      const res = await api.get(`/api/v1/crm/customers/${customerId}/notes`)
      return res.data as NoteDto[]
    },
    enabled: !!customerId,
  })
}
