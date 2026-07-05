import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"

export function useElementCapacity(elementId: string) {
  return useQuery({
    queryKey: queryKeys.networks.elements.capacity(elementId),
    queryFn: async () => {
      const res = await api.get(`/api/v1/network/elements/${elementId}/capacity`)
      return res.data
    },
    enabled: !!elementId,
  })
}
