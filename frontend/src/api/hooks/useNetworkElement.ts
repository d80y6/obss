import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { NetworkElementDto } from '@/api/generated/dto'

export function useNetworkElement(id: string) {
  return useQuery({
    queryKey: queryKeys.networks.elements.detail(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/network/elements/${id}`)
      return res.data as NetworkElementDto
    },
    enabled: !!id,
  })
}
