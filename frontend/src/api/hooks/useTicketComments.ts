import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { TicketCommentDto } from "@/api/generated"

export function useTicketComments(ticketId: string) {
  return useQuery({
    queryKey: queryKeys.tickets.comments(ticketId),
    queryFn: async () => {
      const res = await api.get(`/api/v1/ticketing/tickets/${ticketId}/comments`)
      return res.data as TicketCommentDto[]
    },
    enabled: !!ticketId,
  })
}

export function useAddTicketComment(ticketId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: { content: string; isInternal?: boolean }) => {
      const res = await api.post(`/api/v1/ticketing/tickets/${ticketId}/comments`, data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.tickets.comments(ticketId) })
      queryClient.invalidateQueries({ queryKey: queryKeys.tickets.detail(ticketId) })
    },
  })
}
