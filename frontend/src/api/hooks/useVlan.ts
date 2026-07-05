import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { VlanDto } from '@/api/generated/dto'

export function useVlan(id: string) {
  return useQuery({
    queryKey: queryKeys.networks.vlans.detail(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/network/vlans/${id}`)
      return res.data as VlanDto
    },
    enabled: !!id,
  })
}
