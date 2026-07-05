import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"

export function useDegradedLinks() {
  return useQuery({
    queryKey: queryKeys.networks.links.degraded(),
    queryFn: async () => {
      const res = await api.get("/api/v1/network/links/degraded")
      return res.data
    },
  })
}
