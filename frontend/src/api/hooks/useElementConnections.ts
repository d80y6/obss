import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { NetworkConnectionDto } from '@/api/generated/dto'

export function useElementConnections(elementId: string) {
  return useQuery({
    queryKey: queryKeys.networks.elements.connections(elementId),
    queryFn: async () => {
      const res = await api.get(`/api/v1/network/elements/${elementId}/connections`)
      return res.data as NetworkConnectionDto[]
    },
    enabled: !!elementId,
  })
}
