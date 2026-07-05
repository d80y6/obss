import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { NetworkLinkDto } from '@/api/generated/dto'

export function useNetworkTopology() {
  return useQuery({
    queryKey: queryKeys.networks.topology.list(),
    queryFn: async () => {
      const res = await api.get("/api/v1/network/topology")
      return res.data as NetworkLinkDto[]
    },
  })
}
