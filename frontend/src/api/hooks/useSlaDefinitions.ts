import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { SlaDefinitionDto } from "@/api/generated"

export function useSlaDefinitions() {
  return useQuery({
    queryKey: queryKeys.tickets.slaDefinitions(),
    queryFn: async () => {
      const res = await api.get("/api/v1/ticketing/sla-definitions")
      return res.data as SlaDefinitionDto[]
    },
  })
}

export function useCreateSlaDefinition() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: Record<string, unknown>) => {
      const res = await api.post("/api/v1/ticketing/sla-definitions", data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.tickets.slaDefinitions() })
    },
  })
}

export function useDeleteSlaDefinition() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      await api.delete(`/api/v1/ticketing/sla-definitions/${id}`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.tickets.slaDefinitions() })
    },
  })
}
