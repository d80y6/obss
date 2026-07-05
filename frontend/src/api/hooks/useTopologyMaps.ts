import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"

export function useTopologyMaps() {
  return useQuery({
    queryKey: queryKeys.networks.topology.maps.list(),
    queryFn: async () => {
      const res = await api.get("/api/v1/network/topology/maps")
      return res.data
    },
  })
}
