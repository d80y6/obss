import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { SlaDefinitionDto } from '@/api/generated/dto'

export function useTicketSla(ticketId: string) {
  return useQuery({
    queryKey: queryKeys.tickets.sla(ticketId),
    queryFn: async () => {
      const res = await api.get(`/api/v1/tickets/${ticketId}/sla`)
      return res.data as SlaDefinitionDto
    },
    enabled: !!ticketId,
  })
}
