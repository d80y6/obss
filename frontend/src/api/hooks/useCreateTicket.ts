import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { TicketDto } from '@/api/generated/dto'
import type { CreateTicketCommand } from '@/api/generated'

export function useCreateTicket() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: CreateTicketCommand) => {
      const res = await api.post<TicketDto>("/api/v1/ticketing/tickets", data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.tickets.lists() })
    },
  })
}
