import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { TicketDto } from '@/api/generated/dto'

export function useTicket(id: string) {
  return useQuery({
    queryKey: queryKeys.tickets.detail(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/ticketing/tickets/${id}`)
      return res.data as TicketDto
    },
    enabled: !!id,
  })
}
